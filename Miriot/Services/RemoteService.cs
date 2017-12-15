using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class RemoteService
    {
        private readonly SocketService _socketService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IPlatformService _platformService;
        private List<RomeRemoteSystem> _remoteSystems;
        private RomeRemoteSystem _connectedRemoteSystem;
        private MainViewModel _vm;

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();
        public Action<RomeRemoteSystem> Added { get; set; }

        public RemoteService(SocketService socketService,
            IDispatcherService dispatcherService,
            IPlatformService platformService)
        {
            _socketService = socketService;
            _dispatcherService = dispatcherService;
            _platformService = platformService;
            _remoteSystems = new List<RomeRemoteSystem>();
        }

        public void Attach(MainViewModel vm)
        {
            _vm = vm;
        }

        public async Task<bool> SendAsync(RemoteParameter parameter)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return false;
            }

            try
            {
                await _socketService.SendReceiveMessageAsync(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(parameter));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<T> CommandAsync<T>(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return default(T);
            }

            Debug.WriteLine("Sending " + command + " to " + _connectedRemoteSystem.EndPoint.Address + ":" + _connectedRemoteSystem.EndPoint.Port);
            string response = await _socketService.SendReceiveTcpAsync(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(new RemoteParameter() { Command = command }));

            if (response == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(response);
        }

        public void Command(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return;
            }

            _socketService.SendMessage(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(new RemoteParameter() { Command = command }));
        }

        internal async Task<bool> ConnectAsync(RomeRemoteSystem selectedRemoteSystem)
        {
            _socketService.StopBroadcasting();
            _connectedRemoteSystem = selectedRemoteSystem;
            Command(RemoteCommands.MiriotConnect);
            await Task.Delay(500);

            return true;
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
                    else
                    {

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
            Task.Run(() => _socketService.CreateTcpServer());
        }

        internal void OnLoadUser(User user)
        {
            _dispatcherService.Invoke(async () =>
            {
                await _vm.LoadUser(user);
            });
        }

        private T Deserialize<T>(RemoteParameter parameter)
        {
            return JsonConvert.DeserializeObject<T>(parameter.SerializedData);
        }

        private async Task<string> OnCommandReceivedAsync(RemoteParameter parameter)
        {
            switch (parameter.Command)
            {
                case RemoteCommands.MiriotConfiguring:
                    _dispatcherService.Invoke(() =>
                    {
                        _vm.IsConfiguring = true;
                    });
                    return string.Empty;
                case RemoteCommands.LoadUser:
                    OnLoadUser(Deserialize<User>(parameter));
                    return string.Empty;
                case RemoteCommands.UpdateUser:
                    var user = Deserialize<User>(parameter);
                    var success = await _vm.UpdateUserDataAsync(user);

                    if (success)
                    {
                        _dispatcherService.Invoke(async () =>
                        {
                            _vm.IsConfiguring = false;
                            await _vm.LoadUser(user);
                        });
                    }
                    return JsonConvert.SerializeObject(success);
                case RemoteCommands.GetUser:
                    _dispatcherService.Invoke(() =>
                    {
                        _vm.IsConfiguring = true;
                    });
                    return JsonConvert.SerializeObject(_vm.User);
                case RemoteCommands.GraphService_Initialize:
                    Messenger.Default.Send(new GraphServiceMessage(false));
                    return null;
                case RemoteCommands.GraphService_GetUser:
                    Messenger.Default.Send(new GraphServiceMessage(true));

                    var _graphService = SimpleIoc.Default.GetInstance<IGraphService>();

                    await _graphService.LoginAsync();
                    var graphUser = await _graphService.GetUserAsync();

                    return JsonConvert.SerializeObject(graphUser);
                case RemoteCommands.MiriotConnect:
                    //_socketService.ListenTcp();
                    return null;
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
