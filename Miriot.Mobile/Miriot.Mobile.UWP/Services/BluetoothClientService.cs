using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Miriot.Win10.Services
{
    public class BluetoothClientService : IBluetoothService
    {
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Action<RomeRemoteSystem> Updated { get; set; }
        public Action<string> Removed { get; set; }

        private DeviceWatcher _deviceWatcher = null;
        private StreamSocket _chatSocket = null;
        private DataWriter _chatWriter = null;
        private RfcommDeviceService _chatService = null;
        private BluetoothDevice _bluetoothDevice;
        private ObservableCollection<RomeRemoteSystem> _devices { get; set; }

        private void StopWatcher()
        {
            if (null != _deviceWatcher)
            {
                if ((DeviceWatcherStatus.Started == _deviceWatcher.Status ||
                     DeviceWatcherStatus.EnumerationCompleted == _deviceWatcher.Status))
                {
                    _deviceWatcher.Stop();
                }
                _deviceWatcher = null;
            }
        }

        /// <summary>
        /// When the user presses the run button, query for all nearby unpaired devices
        /// Note that in this case, the other device must be running the Rfcomm Chat Server before being paired.
        /// </summary>
        public Task InitializeAsync()
        {
            if (_deviceWatcher == null)
            {
                Debug.WriteLine("Device watcher started");
                _devices = new ObservableCollection<RomeRemoteSystem>();
                StartUnpairedDeviceWatcher();
            }
            else
            {
                StopWatcher();
            }

            return Task.FromResult(0);
        }

        private async void StartUnpairedDeviceWatcher()
        {
            // Request additional properties
            string[] requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            _deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            // Hook up handlers for the watcher events before starting the watcher
            _deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
            {
                // Make sure device name isn't blank
                if (deviceInfo.Name != "")
                {
                    try
                    {
                        var service = await GattDeviceService.FromIdAsync(deviceInfo.Id);
                        if (service != null)
                        {
                        }
                    }
                    catch (Exception)
                    {

                    }
                    //if (service != null)
                    //{
                    //    var rfcommServices = await service?.GetRfcommServicesForIdAsync(
                    //RfcommServiceId.FromUuid(Constants.SERVICE_UUID), BluetoothCacheMode.Uncached);

                    //    if (rfcommServices?.Services.Count > 0)
                    //    {
                    //if (service.Uuid == Constants.SERVICE_UUID)
                    Discovered?.Invoke(new RomeRemoteSystem(deviceInfo)
                    {
                        Id = deviceInfo.Id,
                        DisplayName = deviceInfo.Name,
                    });
                    //    }
                    //}
                }
            });

            _deviceWatcher.Updated += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                var temp = _devices.ToList();
                foreach (var device in temp)
                {
                    if (device.Id == deviceInfoUpdate.Id)
                    {
                        (device.NativeObject as DeviceInformation).Update(deviceInfoUpdate);

                        var d = _devices.Single(df => df.Id == device.Id);
                        d = new RomeRemoteSystem(device);

                        Updated?.Invoke(d);
                        break;
                    }
                }
            });

            _deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
            });

            _deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                Removed?.Invoke(deviceInfoUpdate.Id);
            });

            _deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
            });

            _deviceWatcher.Start();
        }

        /// <summary>
        /// Invoked once the user has selected the device to connect to.
        /// Once the user has selected the device,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            // Make sure user has selected a device first
            if (system != null)
            {
                Debug.WriteLine("Connecting to remote device. Please wait...");
            }
            else
            {
                Debug.WriteLine("Please select an item to connect to");
                return false;
            }

            // Perform device access checks before trying to get the device.
            // First, we check if consent has been explicitly denied by the user.
            DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(system.Id).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                Debug.WriteLine("This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices");
                return false;
            }

            // If not, try to get the Bluetooth device
            try
            {
                _bluetoothDevice = await BluetoothDevice.FromIdAsync(system.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                StopWatcher();
                return false;
            }

            // If we were unable to get a valid Bluetooth device object,
            // it's most likely because the user has specified that all unpaired devices
            // should not be interacted with.
            if (_bluetoothDevice == null)
            {
                Debug.WriteLine("Bluetooth Device returned null. Access Status = " + accessStatus.ToString());
            }

            if (_bluetoothDevice == null) return false;

            // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call
            var rfcommServices = await _bluetoothDevice?.GetRfcommServicesForIdAsync(
                RfcommServiceId.FromUuid(Constants.SERVICE_UUID), BluetoothCacheMode.Uncached);

            if (rfcommServices?.Services.Count > 0)
            {
                _chatService = rfcommServices.Services[0];
            }
            else
            {
                Debug.WriteLine("Could not discover the chat service on the remote device");
                StopWatcher();
                return false;
            }

            // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service
            var attributes = await _chatService.GetSdpRawAttributesAsync();
            if (!attributes.ContainsKey(Constants.SERVICE_ATTRIBUTE_ID))
            {
                Debug.WriteLine(
                    "The Chat service is not advertising the Service Name attribute (attribute id=0x100). " +
                    "Please verify that you are running the BluetoothRfcommChat server.");
                StopWatcher();
                return false;
            }
            var attributeReader = DataReader.FromBuffer(attributes[Constants.SERVICE_ATTRIBUTE_ID]);
            var attributeType = attributeReader.ReadByte();
            if (attributeType != Constants.SERVICE_ATTRIBUTE_TYPE)
            {
                Debug.WriteLine(
                    "The Chat service is using an unexpected format for the Service Name attribute. " +
                    "Please verify that you are running the BluetoothRfcommChat server.");
                StopWatcher();
                return false;
            }

            var serviceNameLength = attributeReader.ReadByte();

            // The Service Name attribute requires UTF-8 encoding.
            attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

            StopWatcher();

            lock (this)
            {
                _chatSocket = new StreamSocket();
            }
            try
            {
                await _chatSocket.ConnectAsync(_chatService.ConnectionHostName, _chatService.ConnectionServiceName);

                Debug.WriteLine("Connected to : " + attributeReader.ReadString(serviceNameLength) + _bluetoothDevice.Name);
                _chatWriter = new DataWriter(_chatSocket.OutputStream);

                DataReader chatReader = new DataReader(_chatSocket.InputStream);
                ReceiveStringLoop(chatReader);

                return true;
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
            {
                Debug.WriteLine("Please verify that you are running the BluetoothRfcommChat server.");
                StopWatcher();
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
            {
                Debug.WriteLine("Please verify that there is no other RFCOMM connection to the same device.");
                StopWatcher();
            }

            return false;
        }

        /// <summary>
        ///  If you believe the Bluetooth device will eventually be paired with Windows,
        ///  you might want to pre-emptively get consent to access the device.
        ///  An explicit call to RequestAccessAsync() prompts the user for consent.
        ///  If this is not done, a device that's working before being paired,
        ///  will no longer work after being paired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RequestAccessButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure user has given consent to access device
            DeviceAccessStatus accessStatus = await _bluetoothDevice.RequestAccessAsync();

            if (accessStatus != DeviceAccessStatus.Allowed)
            {
                Debug.WriteLine("Access to the device is denied because the application was not granted access");
            }
            else
            {
                Debug.WriteLine("Access granted, you are free to pair devices");
            }
        }

        /// <summary>
        /// Takes the contents of the MessageTextBox and writes it to the outgoing chatWriter
        /// </summary>
        private async void SendMessage(string message)
        {
            try
            {
                if (message.Length != 0)
                {
                    _chatWriter.WriteUInt32((uint)message.Length);
                    _chatWriter.WriteString(message);
                    await _chatWriter.StoreAsync();
                }
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072745)
            {
                // The remote device has disconnected the connection
                Debug.WriteLine("Remote side disconnect: " + ex.HResult.ToString() + " - " + ex.Message);
            }
        }

        private async void ReceiveStringLoop(DataReader chatReader)
        {
            try
            {
                uint size = await chatReader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    Disconnect("Remote device terminated connection - make sure only one instance of server is running on remote device");
                    return;
                }

                uint stringLength = chatReader.ReadUInt32();
                uint actualStringLength = await chatReader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                var res = chatReader.ReadString(stringLength);
                var result = JsonConvert.DeserializeObject<RemoteParameter>(res);

                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (_chatSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        if ((uint)ex.HResult == 0x80072745)
                            Debug.WriteLine("Disconnect triggered by remote device");
                        else if ((uint)ex.HResult == 0x800703E3)
                            Debug.WriteLine("The I/O operation has been aborted because of either a thread exit or an application request.");
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

        private void Disconnect(string disconnectReason)
        {
            if (_chatWriter != null)
            {
                _chatWriter.DetachStream();
                _chatWriter = null;
            }

            if (_chatService != null)
            {
                _chatService.Dispose();
                _chatService = null;
            }
            lock (this)
            {
                if (_chatSocket != null)
                {
                    _chatSocket.Dispose();
                    _chatSocket = null;
                }
            }

            Debug.WriteLine(disconnectReason);
            StopWatcher();
        }
    }
}