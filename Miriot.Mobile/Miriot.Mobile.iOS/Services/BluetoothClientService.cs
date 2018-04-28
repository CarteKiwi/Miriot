using CoreBluetooth;
using CoreFoundation;
using ExternalAccessory;
using Foundation;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Miriot.iOS.Services
{
    public class BluetoothClientService : IBluetoothService
    {
        private const int ConnectionTimeout = 60000;

        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }

        private CBCentralManager _manager;
        private CBPeripheral _connectedPeripheral;
        private IDevice _connectedDevice;

        public async Task Scan()
        {
            Debug.WriteLine("Scanning started");

            CrossBleAdapter.Current
                           //.Scan()
                           .Scan(new ScanConfig { ServiceUuids = { Constants.SERVICE_UUID } })
                           .Subscribe(scanResult =>
                 {
                     var device = $"{scanResult.Device.Name} - {scanResult.Device.Uuid}";
                     Debug.WriteLine($"Discovered {device}");

                     if (!string.IsNullOrEmpty(scanResult.Device.Name))
                         Discovered?.Invoke(new RomeRemoteSystem(scanResult.Device)
                         {
                             DisplayName = scanResult.Device.Name,
                             Id = scanResult.Device.Uuid.ToString()
                         });
                 });
        }

        private void StopScan()
        {
            if (CrossBleAdapter.Current.IsScanning)
                CrossBleAdapter.Current.StopScan();
        }

        public void Disconnect(CBPeripheral peripheral)
        {
            _manager.CancelPeripheralConnection(peripheral);
            Debug.WriteLine($"Device {peripheral.Name} disconnected");
            _connectedPeripheral = null;
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

        public Task InitializeAsync()
        {
            _manager = new CBCentralManager(DispatchQueue.CurrentQueue);
            _manager.UpdatedState += UpdatedState;
            return Task.FromResult(true);
        }

        public CBPeripheral[] GetConnectedDevices(string serviceUuid)
        {
            return _manager.RetrieveConnectedPeripherals(new[] { CBUUID.FromString(serviceUuid) });
        }

        public async Task<CBService> GetService(CBPeripheral peripheral, string serviceUuid)
        {
            var service = this.GetServiceIfDiscovered(peripheral, serviceUuid);
            if (service != null)
            {
                return service;
            }

            var taskCompletion = new TaskCompletionSource<bool>();
            var task = taskCompletion.Task;
            EventHandler<NSErrorEventArgs> handler = (s, e) =>
            {
                if (this.GetServiceIfDiscovered(peripheral, serviceUuid) != null)
                {
                    taskCompletion.SetResult(true);
                }
            };

            try
            {
                peripheral.DiscoveredService += handler;
                //peripheral.Delegate = new SimplePeripheralDelegate();
                //(peripheral.Delegate as SimplePeripheralDelegate).DiscoveredMyService = () =>
                //{
                //    taskCompletion.SetResult(true);
                //};
                peripheral.DiscoverServices();
                await this.WaitForTaskWithTimeout(task, ConnectionTimeout);
                return this.GetServiceIfDiscovered(peripheral, serviceUuid);
            }
            finally
            {
                peripheral.DiscoveredService -= handler;
            }
        }

        public CBService GetServiceIfDiscovered(CBPeripheral peripheral, string serviceUuid)
        {
            serviceUuid = serviceUuid.ToLowerInvariant();
            return peripheral.Services
                ?.FirstOrDefault(x => x.UUID?.Uuid?.ToLowerInvariant() == serviceUuid);
        }

        public async Task<CBCharacteristic[]> GetCharacteristics(CBPeripheral peripheral, CBService service, int scanTime)
        {
            peripheral.DiscoverCharacteristics(service);
            await Task.Delay(scanTime);
            return service.Characteristics;
        }

        public async Task<string> GetAsync(string value)
        {
            try
            {
                //var s = await GetService(_connectedPeripheral, Constants.SERVICE_UUID.ToString());
                var connectedPeripheral = _connectedDevice;
                var rrrr = await _connectedDevice.WriteCharacteristic(Constants.SERVICE_UUID, Constants.SERVICE__WWRITE_UUID, Encoding.ASCII.GetBytes(value));

                var ss = await connectedPeripheral.DiscoverServices();

                //var c = await GetCharacteristics(_connectedPeripheral, s, 10000);
                //var characteristic = c.First(e => e.UUID.ToString() == Constants.SERVICE__WWRITE_UUID.ToString());
                var services = await _connectedDevice.DiscoverServices();
                var s = await _connectedDevice.GetKnownService(Constants.SERVICE_UUID);
                var characteristic = await s.GetKnownCharacteristics(Constants.SERVICE__WWRITE_UUID);

                var res = await characteristic.Write(Encoding.ASCII.GetBytes(value));

                //var c = await _connectedDevice.GetCharacteristicsForService(Constants.SERVICE_UUID).Take(5).ToArray();//.WhenAnyCharacteristicDiscovered().Subscribe(c =>
                //var characteristic = c.First(e => e.Uuid.ToString() == Constants.SERVICE__WWRITE_UUID.ToString());
                //var res = await characteristic.Write(Encoding.ASCII.GetBytes(value));
                var ssssssss = Encoding.ASCII.GetString(res.Data);
                ////{

                //});
                //if (await WriteValue(_connectedPeripheral, characteristic, NSData.FromString(value)))
                //{
                //    characteristic = c.First(e => e.UUID.ToString() == Constants.SERVICE_READ_UUID.ToString());
                //    var res = await ReadValue(_connectedPeripheral, characteristic);
                //    return res;
                //}
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Disconnect(_connectedPeripheral);
            }

            return "";
        }

        public async Task<string> ReadValue(CBPeripheral peripheral, CBCharacteristic characteristic)
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            var task = taskCompletion.Task;
            EventHandler<CBCharacteristicEventArgs> handler = (s, e) =>
            {
                if (e.Characteristic.UUID?.Uuid == characteristic.UUID?.Uuid)
                {
                    taskCompletion.TrySetResult(true);
                }
            };

            try
            {
                peripheral.UpdatedCharacterteristicValue += handler;
                peripheral.ReadValue(characteristic);
                await this.WaitForTaskWithTimeout(task, ConnectionTimeout);
                return characteristic.Value?.ToString();
            }
            finally
            {
                peripheral.UpdatedCharacterteristicValue -= handler;
            }
        }

        public async Task<bool> WriteValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSData value)
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            var task = taskCompletion.Task;
            EventHandler<CBCharacteristicEventArgs> handler = (s, e) =>
            {
                if (e.Characteristic.UUID?.Uuid == characteristic.UUID?.Uuid)
                {
                    taskCompletion.TrySetResult(true);
                }
            };

            try
            {
                peripheral.WroteCharacteristicValue += handler;
                peripheral.WriteValue(value, characteristic, CBCharacteristicWriteType.WithResponse);
                await this.WaitForTaskWithTimeout(task, ConnectionTimeout);
                return task.Result;
            }
            finally
            {
                peripheral.WroteCharacteristicValue -= handler;
            }
        }


        private async Task WaitForTaskWithTimeout(Task task, int timeout)
        {
            await Task.WhenAny(task, Task.Delay(ConnectionTimeout));
            if (!task.IsCompleted)
            {
                throw new TimeoutException();
            }
        }

        private async void UpdatedState(object sender, EventArgs args)
        {
            Debug.WriteLine($"State = {_manager.State}");

            if (_manager.State == CBCentralManagerState.PoweredOn)
            {
                var connectedDevice = this.GetConnectedDevices(Constants.SERVICE_UUID.ToString())
                        ?.FirstOrDefault();

                if (connectedDevice == null)
                    await Scan();
            }
            //this.StateChanged?.Invoke(sender, this.manager.State);
        }
    }
}