using CoreBluetooth;
using CoreFoundation;
using ExternalAccessory;
using Foundation;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Miriot.iOS.Services
{
    public class BluetoothClientService : IBluetoothService
    {
        private const int ConnectionTimeout = 30000;

        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }

        private CBCentralManager _manager;
        private CBPeripheral _connectedPeripheral;
        TaskCompletionSource<bool> _tsc = null;
        TaskCompletionSource<string> _tsc2 = null;

        public async Task Scan(int scanDuration, string serviceUuid = "")
        {
            Debug.WriteLine("Scanning started");
            var uuids = string.IsNullOrEmpty(serviceUuid)
                ? new CBUUID[0]
                : new[] { CBUUID.FromString(serviceUuid) };
            _manager.ScanForPeripherals(uuids);

            await Task.Delay(scanDuration);
            this.StopScan();
        }

        public void StopScan()
        {
            _manager.StopScan();
            Debug.WriteLine("Scanning stopped");
        }

        public async Task ConnectTo(CBPeripheral peripheral)
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            var task = taskCompletion.Task;
            EventHandler<CBPeripheralEventArgs> connectedHandler = (s, e) =>
            {
                if (e.Peripheral.Identifier?.ToString() == peripheral.Identifier?.ToString())
                {
                    _connectedPeripheral = e.Peripheral;
                    taskCompletion.SetResult(true);
                }
            };

            try
            {
                _manager.ConnectedPeripheral += connectedHandler;
                _manager.ConnectPeripheral(peripheral);
                await this.WaitForTaskWithTimeout(task, ConnectionTimeout);
                Debug.WriteLine($"Bluetooth device connected = {peripheral.Name}");
            }
            finally
            {
                _manager.ConnectedPeripheral -= connectedHandler;
            }
        }

        public void Disconnect(CBPeripheral peripheral)
        {
            _manager.CancelPeripheralConnection(peripheral);
            Debug.WriteLine($"Device {peripheral.Name} disconnected");
        }

        public async Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            await ConnectTo((CBPeripheral)system.NativeObject);
            return true;
        }

        public Task InitializeAsync()
        {
            _manager = new CBCentralManager(DispatchQueue.CurrentQueue);
            _manager.UpdatedState += UpdatedState;
            _manager.DiscoveredPeripheral += DiscoveredPeripheral;
            return Task.FromResult(true);
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
                peripheral.DiscoverServices(new[] { CBUUID.FromString(serviceUuid) });
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
                var s = await GetService(_connectedPeripheral, Constants.SERVICE_UUID.ToString());

                var c = await GetCharacteristics(_connectedPeripheral, s, 10000);
                var characteristic = c.First(e => e.UUID.ToString() == Constants.SERVICE__WWRITE_UUID.ToString());

                if (await WriteValue(_connectedPeripheral, characteristic, NSData.FromString(value)))
                {
                    characteristic = c.First(e => e.UUID.ToString() == Constants.SERVICE_READ_UUID.ToString());
                    var res = await ReadValue(_connectedPeripheral, characteristic);
                    return res;
                }
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
                await Scan(30000, Constants.SERVICE_UUID.ToString());
            }
            //this.StateChanged?.Invoke(sender, this.manager.State);
        }

        public void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            var device = $"{args.Peripheral.Name} - {args.Peripheral.Identifier?.Description}";
            Debug.WriteLine($"Discovered {device}");

            if (!string.IsNullOrEmpty(args.Peripheral.Name))
                Discovered?.Invoke(new RomeRemoteSystem(args.Peripheral)
                {
                    DisplayName = args.Peripheral.Name,
                    Id = args.Peripheral.Identifier.ToString()
                });
        }
    }
}