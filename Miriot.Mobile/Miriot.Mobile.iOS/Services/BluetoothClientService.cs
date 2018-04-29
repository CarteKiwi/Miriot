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
        private const int ConnectionTimeout = 60000;

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

                         if (data.Contains("Miriot"))
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

        public void Initialize()
        {
            CrossBleAdapter.Current.WhenStatusChanged().Subscribe(s =>
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
                    Scan();
            });
        }

        public async Task<string> GetAsync(string value)
        {
            try
            {
                var service = await _connectedDevice.GetKnownService(Constants.SERVICE_UUID);

                var writeCharacteristic = await service.GetKnownCharacteristics(Constants.SERVICE__WWRITE_UUID);
                await writeCharacteristic.Write(Encoding.ASCII.GetBytes(value));

                var readCharacteristic = await service.GetKnownCharacteristics(Constants.SERVICE_READ_UUID);
                var result = await readCharacteristic.Read();

                return Encoding.ASCII.GetString(result.Data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAsync failed: " + ex.Message);
                return string.Empty;
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