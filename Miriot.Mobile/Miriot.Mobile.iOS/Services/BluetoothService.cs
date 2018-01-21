using Miriot.Model;
using Miriot.Services;
using System;
using System.Threading.Tasks;

namespace Miriot.iOS.Services
{
    public class BluetoothService : IBluetoothService
    {
        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        public Action<RomeRemoteSystem> Discovered { get; set; }

        public Task<bool> ConnectAsync(RomeRemoteSystem system)
        {
            return Task.FromResult(false);
        }

        public Task InitializeAsync()
        {
            return Task.FromResult(0);
        }
    }
}