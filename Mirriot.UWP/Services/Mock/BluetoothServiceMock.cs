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
    public class BluetoothServiceMock : IBluetoothService
    {
        public Action<RomeRemoteSystem> Discovered { get; set; }
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }

        public Task InitializeAsync()
        {
            Task.Run(async () =>
            {
                SimulateData(RemoteCommands.GetUser);
                await Task.Delay(5000);
                SimulateData(RemoteCommands.GraphService_Initialize);
                await Task.Delay(2000);
                SimulateData(RemoteCommands.GraphService_GetCode);
                await Task.Delay(5000);
                SimulateData(RemoteCommands.GoToCameraPage);
                await Task.Delay(5000);
                SimulateData(RemoteCommands.CameraAdjustBrightness);
            });

            return Task.FromResult(true);
        }

        private async void SimulateData(RemoteCommands cmd)
        {
            var parameter = new RemoteParameter() { Command = cmd };

            string serializedData = await CommandReceived(parameter);
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
        }

        public Task SendAsync(string parameter)
        {
            throw new NotImplementedException();
        }
    }
}
