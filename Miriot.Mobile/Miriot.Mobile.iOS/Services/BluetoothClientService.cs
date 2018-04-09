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
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }

        private CBCentralManager _manager;
        private CBPeripheral _connectedPeripheral;
        TaskCompletionSource<bool> _tsc = null;
        TaskCompletionSource<string> _tsc2 = null;

        public async Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            _tsc = new TaskCompletionSource<bool>();

            _manager.StopScan();
            myDel.Connected = (s) =>
            {
                if (e.Peripheral.Identifier?.ToString() == peripheral.Identifier?.ToString())
                {
                    _tsc.TrySetResult(true);
                }
            };
            _connectedPeripheral = (CBPeripheral)system.NativeObject;

            _manager.ConnectPeripheral((CBPeripheral)system.NativeObject);

            return await _tsc.Task;
        }

        MySimpleCBCentralManagerDelegate myDel;

        public Task InitializeAsync() 
        { 
            myDel = new MySimpleCBCentralManagerDelegate(); 
            _manager = new CBCentralManager(myDel, DispatchQueue.CurrentQueue); 
            myDel.Discovered = (s) => 
            { 
                Discovered?.Invoke(s); 
            }; 
 
            return Task.FromResult(false); 
        } 
 

        public async Task<string> GetAsync(string value)
        {
            var service = _connectedPeripheral.Services.FirstOrDefault(e => e.UUID.ToString() == Constants.SERVICE_UUID.ToString());

            if(service != null)
            {
                var characteristic = service.Characteristics.FirstOrDefault(c => c.UUID.ToString() == Constants.SERVICE__WWRITE_UUID.ToString());
       
                _tsc2 = new TaskCompletionSource<string>();
                myDel.ValueUpdated = async (v)=>
                {
                    var res = await ReadValue(_connectedPeripheral, characteristic);
                    _tsc2.TrySetResult(res);
                };

                _connectedPeripheral.WriteValue(NSData.FromString(value), characteristic, CBCharacteristicWriteType.WithResponse);
                //characteristic.Value = new NSData(value, NSDataBase64DecodingOptions.None);

                return await _tsc2.Task;
                //_connectedPeripheral.WriteValue(new NSData("COUCOU", NSDataBase64DecodingOptions.None), null);
            }

            return ("");

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
                return characteristic.Value.ToString();
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

        private const int ConnectionTimeout = 10000; 

        private async Task WaitForTaskWithTimeout(Task task, int timeout)
        {
            await Task.WhenAny(task, Task.Delay(ConnectionTimeout));
            if (!task.IsCompleted)
            {
                throw new TimeoutException();
            }
        }
    }

    public class MySimpleCBCentralManagerDelegate : CBCentralManagerDelegate
    {
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Action<CBPeripheral> Connected { get; set; }
        public Action<NSData> ValueUpdated { get; set; }

        public override void UpdatedState(CBCentralManager mgr)
        {
            if (mgr.State == CBCentralManagerState.PoweredOn)
            {
                //Passing in null scans for all peripherals. Peripherals can be targeted by using CBUIIDs
                CBUUID[] cbuuids = { CBUUID.FromString(Constants.SERVICE_UUID.ToString()) };
                mgr.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral
                //Timeout after 30 seconds
                //var timer = new Timer(30 * 1000);
                //timer.Elapsed += (sender, e) => mgr.StopScan();
            }
            else
            {
                //Invalid state -- Bluetooth powered down, unavailable, etc.
                System.Console.WriteLine("Bluetooth is not available");
            }
        }

        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI)
        {
            Console.WriteLine("Discovered {0}, data {1}, RSSI {2}", peripheral.Name, advertisementData, RSSI);

            if (!string.IsNullOrEmpty(peripheral.Name))
                Discovered?.Invoke(new RomeRemoteSystem(peripheral)
                {
                    DisplayName = peripheral.Name,
                    Id = peripheral.Identifier.ToString()
                });
        }

        public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            Connected?.Invoke(peripheral);

            var connectedPeripheral = peripheral;

            if (connectedPeripheral.Delegate == null)
            {
                connectedPeripheral.Delegate = new SimplePeripheralDelegate();
                ((SimplePeripheralDelegate)connectedPeripheral.Delegate).ValueUpdated = (a) =>
                {
                    ValueUpdated?.Invoke(a);
                };
                connectedPeripheral.DiscoverServices();
            }
        }
    }

    public class SimplePeripheralDelegate : CBPeripheralDelegate
    {
        public Action<NSData> ValueUpdated { get; set; }
       
        public override void DiscoveredService(CBPeripheral peripheral, NSError error)
        {
            System.Console.WriteLine("Discovered a service");
            foreach (var service in peripheral.Services)
            {
                if(service.UUID.ToString() == Constants.SERVICE_UUID.ToString())
                {
                    Console.WriteLine("OLA");
                }

                Console.WriteLine(service.ToString());
                peripheral.DiscoverCharacteristics(service);
            }
        }

        public override void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
        {
            System.Console.WriteLine("Discovered characteristics of " + peripheral);
            foreach (var c in service.Characteristics)
            {
                Console.WriteLine(c.ToString());
                peripheral.ReadValue(c);
            }
        }

		public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
		{
            ValueUpdated?.Invoke(characteristic.Value);
		}

		public override void UpdatedValue(CBPeripheral peripheral, CBDescriptor descriptor, NSError error)
        {
            Console.WriteLine("Value of characteristic from desc " + descriptor.Characteristic + " is " + descriptor.Value);
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            ValueUpdated?.Invoke(characteristic.Value);
            Console.WriteLine("Value of characteristic " + characteristic.ToString() + " is " + characteristic.Value);
        }

        public override void UpdatedNotificationState(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            Console.WriteLine("notifffff of characteristic " + characteristic.ToString() + " is " + characteristic.Value);
           
        }
    }
}