using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Plugin.BluetoothLE;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.iOS.Services
{
    public class BluetoothClientService : IBluetoothService
    {
        private const int ConnectionTimeout = 30000;

        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }

        private IDevice _connectedDevice;

        private void Scan()
        {
            Debug.WriteLine("Scanning started");

            CrossBleAdapter.Current
                           .Scan()
                           //.Scan(new ScanConfig { ServiceUuids = { Constants.SERVICE_UUID } })
                           .Subscribe(scanResult =>
                 {
                     var device = $"{scanResult.Device.Name} - {scanResult.Device.Uuid}";
                     Debug.WriteLine($"Discovered {device}");

                     if (!string.IsNullOrEmpty(scanResult.Device.Name)
                         && scanResult.AdvertisementData.ManufacturerData != null)
                     {
                         var data = Encoding.ASCII.GetString(scanResult.AdvertisementData.ManufacturerData);

                         if (scanResult.Device.Name.Contains("Miriot"))
                         //if (data.Contains("Miriot"))
                         {
                             Discovered?.Invoke(new RomeRemoteSystem(scanResult.Device)
                             {
                                 DisplayName = scanResult.Device.Name,
                                 Id = scanResult.Device.Uuid.ToString()
                             });
                         }
                     }
                 });
        }

        private void StopScan()
        {
            if (CrossBleAdapter.Current.IsScanning)
                CrossBleAdapter.Current.StopScan();
        }

        public async Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            try
            {
                StopScan();

                if (_connectedDevice != null && _connectedDevice.Status == ConnectionStatus.Disconnected)
                {
                    Debug.WriteLine($"Cancelling connection & reconnect");
                    _connectedDevice.CancelConnection();
                }

                Debug.WriteLine($"Connecting...");

                _connectedDevice = await ((IDevice)system.NativeObject)
                    .ConnectWait()
                    .Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));

                return _connectedDevice != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to connect : " + ex.Message);
                return false;
            }
        }

        public Task InitializeAsync()
        {
            CrossBleAdapter.Current.WhenStatusChanged().Subscribe(s =>
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
                    Scan();
            });

            return Task.FromResult(true);
        }

        public async Task<string> GetAsync(string value)
        {
            try
            {
                Debug.WriteLine("Get service");
                var service = await _connectedDevice.GetKnownService(Constants.SERVICE_UUID).Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));

                Debug.WriteLine("Get write characteristic");
                var writeCharacteristic = await service.GetKnownCharacteristics(Constants.SERVICE__WWRITE_UUID).Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));
                Debug.WriteLine("Write characteristic");
                await writeCharacteristic.Write(Encoding.ASCII.GetBytes(value)).Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));

                Debug.WriteLine("Get read characteristic");
                var readCharacteristic = await service.GetKnownCharacteristics(Constants.SERVICE_READ_UUID).Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));
                Debug.WriteLine("Read characteristic");
                var result = await readCharacteristic.Read().Timeout(TimeSpan.FromMilliseconds(ConnectionTimeout));

                return Encoding.ASCII.GetString(result.Data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAsync failed: " + ex.Message);
                return string.Empty;
            }
            finally
            {

            }
        }

        public async Task SendAsync(string value)
        {
            try
            {
                var service = await _connectedDevice.GetKnownService(Constants.SERVICE_UUID);
                var writeCharacteristic = await service.GetKnownCharacteristics(Constants.SERVICE__WWRITE_UUID);
                await writeCharacteristic.Write(Encoding.ASCII.GetBytes(value));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SendAsync failed: " + ex.Message);
            }
        }

        public void StopAdv()
        {
            throw new NotImplementedException();
        }
    }
}