using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Miriot.Win10.Services
{
    public class BluetoothService : IBluetoothService
    {
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }

        private RfcommServiceProvider _provider;
        private StreamSocketListener _listener;
        private StreamSocket _socket;
        private DataWriter _writer;

        public async Task InitializeAsync()
        {
            // Initialize the provider for the hosted RFCOMM service
            _provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(Constants.SERVICE_UUID));

            // Create a listener for this service and start listening
            _listener = new StreamSocketListener();
            _listener.ConnectionReceived += OnConnectionReceivedAsync;

            await _listener.BindServiceNameAsync(_provider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start advertising
            InitializeServiceSdpAttributes(_provider);
            _provider.StartAdvertising(_listener, true);
        }

        void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        {
            var writer = new DataWriter();

            // First write the attribute type
            writer.WriteByte(Constants.SERVICE_ATTRIBUTE_TYPE);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            writer.WriteByte((byte)Constants.SERVICE_NAME.Length);

            // The UTF-8 encoded Service Name value.
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.WriteString(Constants.SERVICE_NAME);

            var data = writer.DetachBuffer();
            provider.SdpRawAttributes.Add(Constants.SERVICE_ATTRIBUTE_ID, data);
        }

        private async void OnConnectionReceivedAsync(
            StreamSocketListener listener,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            // Stop advertising/listening so that we're only serving one client
            _provider.StopAdvertising();
            listener.Dispose();
            listener = null;

            _socket = args.Socket;

            // Note - this is the supported way to get a Bluetooth device from a given socket
            var remoteDevice = await BluetoothDevice.FromHostNameAsync(_socket.Information.RemoteHostName);

            _writer = new DataWriter(_socket.OutputStream);
            var reader = new DataReader(_socket.InputStream);
            bool remoteDisconnection = false;

            Debug.WriteLine("Connected to Client: " + remoteDevice.Name);

            // Infinite read buffer loop
            while (true)
            {
                try
                {
                    // Based on the protocol we've defined, the first uint is the size of the message
                    uint readLength = await reader.LoadAsync(sizeof(uint));

                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    uint currentLength = reader.ReadUInt32();

                    // Load the rest of the message since you already know the length of the data expected.  
                    readLength = await reader.LoadAsync(currentLength);

                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    string message = reader.ReadString(currentLength);

                    var parameter = JsonConvert.DeserializeObject<RemoteParameter>(message);

                    string serializedData = await CommandReceived(parameter);
                }
                // Catch exception HRESULT_FROM_WIN32(ERROR_OPERATION_ABORTED).
                catch (Exception ex) when ((uint)ex.HResult == 0x800703E3)
                {
                    Debug.WriteLine("Client Disconnected Successfully");
                    break;
                }
            }

            reader.DetachStream();
            if (remoteDisconnection)
            {
                Disconnect();
                Debug.WriteLine("Client disconnected");
            }
        }

        private async void Disconnect()
        {
            if (_provider != null)
            {
                _provider.StopAdvertising();
                _provider = null;
            }

            if (_listener != null)
            {
                _listener.Dispose();
                _listener = null;
            }

            if (_writer != null)
            {
                _writer.DetachStream();
                _writer = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        private async void SendMessage(string message)
        {
            // There's no need to send a zero length message
            if (message.Length != 0)
            {
                // Make sure that the connection is still up and there is a message to send
                if (_socket != null)
                {
                    _writer.WriteUInt32((uint)message.Length);
                    _writer.WriteString(message);

                    Debug.WriteLine("Sent: " + message);

                    await _writer.StoreAsync();
                }
                else
                {
                    Debug.WriteLine("No clients connected, please wait for a client to connect before attempting to send a message");
                }
            }
        }

        public Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            throw new NotImplementedException();
        }
    }
}
