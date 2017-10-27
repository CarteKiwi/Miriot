using Microsoft.ConnectedDevices;
using Miriot.Model;
using Miriot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Mobile.Droid.Services
{
    public class RomeService : IRomeService
    {
        private RemoteSystemWatcher _remoteSystemWatcher;
        private List<RomeRemoteSystem> _remoteSystems;

        public bool IsInitialized { get; set; }

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();

        public Action<RomeRemoteSystem> Added { get; set; }

        public async Task InitializeAsync()
        {
            RemoteSystem.RequestAccessAsync();

            _remoteSystems = new List<RomeRemoteSystem>();

            // construct watcher with the list
            _remoteSystemWatcher = RemoteSystem.CreateWatcher();
            _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcherOnRemoteSystemAdded;
            _remoteSystemWatcher.Start();

            await Task.FromResult(0);
        }

        private void RemoteSystemWatcherOnRemoteSystemAdded(RemoteSystemWatcher watcher, RemoteSystemAddedEventArgs args)
        {
            var system = ToRomeRemoteSystem(args.P0);

            _remoteSystems.Add(system);
            Added?.Invoke(system);
        }

        private static RomeRemoteSystem ToRomeRemoteSystem(RemoteSystem rs)
        {
            return new RomeRemoteSystem(rs)
            {
                DisplayName = rs.DisplayName,
                Id = rs.Id,
                Kind = rs.Kind.ToString(),
                Status = rs.Status.ToString()
            };
        }

        public Task<bool> RemoteLaunchUri(RomeRemoteSystem remoteSystem, Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendCommandAsync(RomeRemoteSystem remoteSystem, string command)
        {
            throw new NotImplementedException();
        }
    }
}
