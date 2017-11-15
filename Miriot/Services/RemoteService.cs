using Miriot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class RemoteService
    {
        private readonly SocketService _socketService;
        private Timer _discoveryTimer;
        private List<RomeRemoteSystem> _remoteSystems;
        private RomeRemoteSystem _connectedRemoteSystem;

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();
        public Action<RomeRemoteSystem> Added { get; set; }

        public RemoteService(SocketService socketService)
        {
            _socketService = socketService;
            _remoteSystems = new List<RomeRemoteSystem>();
        }

        public async Task<T> CommandAsync<T>(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return default(T);
            }

            string response = await _socketService.SendReceiveMessageAsync(_connectedRemoteSystem.EndPoint, command.ToString());

            return JsonConvert.DeserializeObject<T>(response);
        }

        public void Command(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return;
            }

            _socketService.SendMessage(_connectedRemoteSystem.EndPoint, command.ToString());
        }

        internal Task<bool> ConnectAsync(RomeRemoteSystem selectedRemoteSystem)
        {
            _socketService.StopBroadcasting();
            _connectedRemoteSystem = selectedRemoteSystem;

            return Task.FromResult(true);
        }

        public void Discover()
        {
            try
            {
                _socketService.Broadcast();

                _socketService.Discovered = (response, ipEndPoint) =>
                {
                    var remoteSystem = _remoteSystems.FirstOrDefault(r => r.EndPoint.Address.ToString() == ipEndPoint.Address.ToString());

                    if (remoteSystem == null)
                    {
                        var system = new RomeRemoteSystem(null)
                        {
                            DisplayName = response,
                            EndPoint = ipEndPoint
                        };

                        _remoteSystems.Add(system);
                        Added?.Invoke(system);
                    }
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        internal void Listen()
        {
            Task.Run(() => _socketService.BroadcastListener());
        }
    }
}
