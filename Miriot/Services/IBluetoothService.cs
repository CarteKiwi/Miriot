using Miriot.Model;
using System;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IBluetoothService
    {
        void Initialize();
        Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
        Action<RomeRemoteSystem> Discovered { get; set; }
        Task<bool> ConnectAsync(RomeRemoteSystem system);
        Task<string> GetAsync(string parameter);
        Task SendAsync(string parameter);
        void StopAdv();
    }
}
