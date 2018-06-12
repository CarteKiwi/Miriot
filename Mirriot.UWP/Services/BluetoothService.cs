using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Miriot.Win10.Services
{
    public class BluetoothService : IBluetoothService
    {
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }

        private BluetoothLEAdvertisementPublisher _publisher;
        private GattServiceProvider _serviceProvider;
        private StreamSocketListener _listener;
        private StreamSocket _socket;
        private DataWriter _writer;
        private GattLocalCharacteristic _writeCharacteristic;
        private GattLocalCharacteristic _readCharacteristic;

        public async Task InitializeAsync()
        {
            App.Current.Suspending -= Current_Suspending;
            App.Current.Suspending += Current_Suspending;
            //CrossServer();
            //return;

            GattServiceProviderResult result = await GattServiceProvider.CreateAsync(Constants.SERVICE_UUID);

            if (result.Error == BluetoothError.Success)
            {
                _serviceProvider = result.ServiceProvider;
                byte[] value = new byte[] { 0x21 };
                var readParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = (GattCharacteristicProperties.Read),
                    ReadProtectionLevel = GattProtectionLevel.Plain,
                };

                GattLocalCharacteristicResult characteristicResult = await _serviceProvider.Service.CreateCharacteristicAsync(Constants.SERVICE_READ_UUID, readParameters);
                if (characteristicResult.Error != BluetoothError.Success)
                {
                    // An error occurred.
                    return;
                }

                _readCharacteristic = characteristicResult.Characteristic;
                _readCharacteristic.ReadRequested += ReadCharacteristic_ReadRequested;

                var writeParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = (GattCharacteristicProperties.Write),
                    WriteProtectionLevel = GattProtectionLevel.Plain,
                };

                characteristicResult = await _serviceProvider.Service.CreateCharacteristicAsync(Constants.SERVICE__WWRITE_UUID, writeParameters);
                if (characteristicResult.Error != BluetoothError.Success)
                {
                    // An error occurred.
                    return;
                }
                _writeCharacteristic = characteristicResult.Characteristic;
                _writeCharacteristic.ReadRequested += ReadCharacteristic_ReadRequested;
                _writeCharacteristic.WriteRequested += WriteCharacteristic_WriteRequested;
            }

            GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

            _serviceProvider.StartAdvertising(advParameters);

            // Add custom data to the advertisement
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 0xFFFE;

            var writer = new DataWriter();
            writer.WriteString("Miriot");

            // Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
            // Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            var adv = new BluetoothLEAdvertisement();
            adv.ManufacturerData.Add(manufacturerData);

            _publisher = new BluetoothLEAdvertisementPublisher(adv);

            //// Add the manufacturer data to the advertisement publisher:
            //_publisher.Advertisement.ManufacturerData.Add(manufacturerData);

            _publisher.Start();
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            if (_readCharacteristic != null)
            {
                _readCharacteristic.ReadRequested -= ReadCharacteristic_ReadRequested;
                _readCharacteristic.WriteRequested -= WriteCharacteristic_WriteRequested;
            }

            if (_writeCharacteristic != null)
            {
                _writeCharacteristic.ReadRequested -= ReadCharacteristic_ReadRequested;
                _writeCharacteristic.WriteRequested -= WriteCharacteristic_WriteRequested;
            }

            _readCharacteristic = null;
            _writeCharacteristic = null;
            _publisher?.Stop();
            StopAdv();
            _serviceProvider = null;
        }

        private void CrossServer()
        {
            var server = CrossBleAdapter.Current.CreateGattServer();
            var service = server.CreateService(Constants.SERVICE_UUID, true);

            var characteristic = service.AddCharacteristic(
                Constants.SERVICE__WWRITE_UUID,
                CharacteristicProperties.Read | CharacteristicProperties.Write | CharacteristicProperties.WriteNoResponse,
                GattPermissions.Read | GattPermissions.Write
            );

            var notifyCharacteristic = service.AddCharacteristic
            (
                Constants.SERVICE_READ_UUID,
                CharacteristicProperties.Indicate | CharacteristicProperties.Notify,
                GattPermissions.Read | GattPermissions.Write
            );

            IDisposable notifyBroadcast = null;
            notifyCharacteristic.WhenDeviceSubscriptionChanged().Subscribe(e =>
            {
                var @event = e.IsSubscribed ? "Subscribed" : "Unsubcribed";

                if (notifyBroadcast == null)
                {
                    notifyBroadcast = Observable
                        .Interval(TimeSpan.FromSeconds(1))
                        .Where(x => notifyCharacteristic.SubscribedDevices.Count > 0)
                        .Subscribe(_ =>
                        {
                            Debug.WriteLine("Sending Broadcast");
                            var dt = DateTime.Now.ToString("g");
                            var bytes = Encoding.UTF8.GetBytes(dt);
                            notifyCharacteristic.Broadcast(bytes);
                        });
                }
            });

            characteristic.WhenReadReceived().Subscribe(x =>
            {
                var write = "HELLO";

                // you must set a reply value
                x.Value = Encoding.UTF8.GetBytes(write);

                x.Status = GattStatus.Success; // you can optionally set a status, but it defaults to Success
            });
            characteristic.WhenWriteReceived().Subscribe(x =>
            {
                var write = Encoding.UTF8.GetString(x.Value, 0, x.Value.Length);
                // do something value
            });

            server.AddService(service);

            //await server.Start(new AdvertisementData
            //{
            //    LocalName = "TestServer"
            //});
        }

        private string _data;

        private async void WriteCharacteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {

            var deferral = args.GetDeferral();

            try
            {
                var request = await args.GetRequestAsync();
                var reader = DataReader.FromBuffer(request.Value);

                string message = reader.ReadString(request.Value.Length);

                var parameter = JsonConvert.DeserializeObject<RemoteParameter>(message);

                string serializedData = await CommandReceived(parameter);
                _data = serializedData;

                if (request.Option == GattWriteOption.WriteWithResponse)
                {
                    request.Respond();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void ReadCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                var dw = new DataWriter();

                if (_data != null)
                    dw.WriteString(_data);

                var request = await args.GetRequestAsync();
                request.RespondWithValue(dw.DetachBuffer());
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
            }
        }

        public Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAsync(string parameter)
        {
            throw new NotImplementedException();
        }

        public void StopAdv()
        {
            try
            {
                _serviceProvider?.StopAdvertising();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Task SendAsync(string parameter)
        {
            throw new NotImplementedException();
        }
    }
}
