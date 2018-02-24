using CoreBluetooth;
using CoreFoundation;
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

        public async Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            _tsc = new TaskCompletionSource<bool>();

            _manager.StopScan();
            myDel.Connected = (s) =>
            {
                _tsc.TrySetResult(true);
            };
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

        public void SendReceiveAsync(string value)
        {
            var service = _connectedPeripheral.Services.FirstOrDefault(e => e.UUID.ToString() == Constants.SERVICE_UUID.ToString());

            if(service != null)
            {
                _connectedPeripheral.WriteValue(new NSData("COUCOU", NSDataBase64DecodingOptions.None), null);
            }
        }

        private void _manager_DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Peripheral.Name))
                Discovered?.Invoke(new RomeRemoteSystem(e.Peripheral)
                {
                    DisplayName = e.Peripheral.Name,
                    Id = e.Peripheral.Identifier.ToString()
                });
        }
    }

    public class MySimpleCBCentralManagerDelegate : CBCentralManagerDelegate
    {
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Action<CBPeripheral> Connected { get; set; }

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
                connectedPeripheral.DiscoverServices();
            }
        }
    }

    public class SimplePeripheralDelegate : CBPeripheralDelegate
    {
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

        public override void UpdatedValue(CBPeripheral peripheral, CBDescriptor descriptor, NSError error)
        {
            Console.WriteLine("Value of characteristic " + descriptor.Characteristic + " is " + descriptor.Value);
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            Console.WriteLine("Value of characteristic " + characteristic.ToString() + " is " + characteristic.Value);
        }
    }
}