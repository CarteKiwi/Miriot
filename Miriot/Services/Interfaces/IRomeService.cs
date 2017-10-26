using Miriot.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miriot.Services.Interfaces
{
    public interface IRomeService
    {
        Action<RomeRemoteSystem> Added { get; set; }
        bool IsInitialized { get; }
        IReadOnlyList<RomeRemoteSystem> RemoteSystems { get; }
        Task InitializeAsync();
        Task<bool> RemoteLaunchUri(RomeRemoteSystem remoteSystem, Uri uri);
        Task<bool> SendCommandAsync(RomeRemoteSystem remoteSystem, string command);
    }
}
