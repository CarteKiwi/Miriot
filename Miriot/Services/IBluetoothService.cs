using Miriot.Model;
using System;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IBluetoothService
    {
        Task InitializeAsync();
        Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        Action<RomeRemoteSystem> Discovered { get; set; }
        Task ConnectAsync(RomeRemoteSystem system);
    }
}
