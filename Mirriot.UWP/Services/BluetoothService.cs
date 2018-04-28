using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

        private GattServiceProvider _serviceProvider;
        private StreamSocketListener _listener;
        private StreamSocket _socket;
        private DataWriter _writer;
        private GattLocalCharacteristic _writeCharacteristic;
        private GattLocalCharacteristic _readCharacteristic;

        public async Task InitializeAsync()
        {
            GattServiceProviderResult result = await GattServiceProvider.CreateAsync(Constants.SERVICE_UUID);

            if (result.Error == BluetoothError.Success)
            {
                var tt = new DataWriter();
                tt.WriteString("coucou");
                var titi = tt.DetachBuffer();

                var b = Encoding.UTF8.GetBytes("hello");

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

                //var t = new JsonTextReader();

                //            var buffer = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(
                //"What fools these mortals be", Windows.Security.Cryptography.BinaryStringEncoding.Utf8);

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
                //var dw = new DataWriter();
                //dw.WriteString(serializedData);
                
                //await _writeCharacteristic.NotifyValueAsync(dw.DetachBuffer());

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

        public Task<string> GetAsync(string parameter)
        {
            throw new NotImplementedException();
        }

        public void StopAdv()
        {
            _serviceProvider.StopAdvertising();
        }
    }
}
