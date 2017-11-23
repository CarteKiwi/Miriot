using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class RemoteService
    {
        private readonly SocketService _socketService;
        private readonly IPlatformService _platformService;
        private List<RomeRemoteSystem> _remoteSystems;
        private RomeRemoteSystem _connectedRemoteSystem;

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();
        public Action<RomeRemoteSystem> Added { get; set; }

        public RemoteService(SocketService socketService, IPlatformService platformService)
        {
            _socketService = socketService;
            _platformService = platformService;
            _remoteSystems = new List<RomeRemoteSystem>();
        }

        public async Task<T> CommandAsync<T>(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return default(T);
            }

                Debug.WriteLine("Sending " + command + " to " + _connectedRemoteSystem.EndPoint.Address);
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

                _socketService.Discovered = (system) =>
                {
                    var remoteSystem = _remoteSystems.FirstOrDefault(r => r.EndPoint.Address.ToString() == system.EndPoint.Address.ToString());

                    if (remoteSystem == null)
                    {
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
            _socketService.CommandReceived = OnCommandReceivedAsync;
            Task.Run(() => _socketService.BroadcastListener());
        }

        private async Task<string> OnCommandReceivedAsync(RemoteCommands cmd)
        {
            switch (cmd)
            {
                case RemoteCommands.GetUser:
                    var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                    return JsonConvert.SerializeObject(vm.User);
                case RemoteCommands.GraphService_Initialize:
                    Messenger.Default.Send(new GraphServiceMessage(false));
                    return null;
                case RemoteCommands.GraphService_GetUser:
                    Messenger.Default.Send(new GraphServiceMessage(true));

                    var _graphService = SimpleIoc.Default.GetInstance<IGraphService>();

                    await _graphService.LoginAsync();
                    var user = await _graphService.GetUserAsync();

                    return JsonConvert.SerializeObject(user);
                case RemoteCommands.MiriotDiscovery:
                default:
                    // Reply back
                    var id = _platformService.GetSystemIdentifier();
                    var sys = new RomeRemoteSystem(null)
                    {
                        Id = id,
                        DisplayName = Environment.MachineName
                    };
                    return JsonConvert.SerializeObject(sys);
            }
        }
    }
}
