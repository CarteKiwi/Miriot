using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
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
        private readonly IBluetoothService _bluetoothService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IPlatformService _platformService;
        private List<RomeRemoteSystem> _remoteSystems;
        private RomeRemoteSystem _connectedRemoteSystem;
        private MainViewModel _vm;

        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();

        public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }

        public Action<RomeRemoteSystem> Added { get; set; }

        public RemoteService(
            IBluetoothService socketService,
            IDispatcherService dispatcherService,
            IPlatformService platformService)
        {
            _bluetoothService = socketService;
            _dispatcherService = dispatcherService;
            _platformService = platformService;
            _remoteSystems = new List<RomeRemoteSystem>();

            CommandReceived = OnCommandReceivedAsync;
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
                //await _socketService.SendReceiveMessageAsync(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(parameter));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        // Rename to Get
        public async Task<T> CommandAsync<T>(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return default(T);
            }

            //Debug.WriteLine("Sending " + command + " to " + _connectedRemoteSystem.EndPoint.Address + ":" + _connectedRemoteSystem.EndPoint.Port);
            //string response = await _socketService.SendReceiveMessageAsync(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(new RemoteParameter() { Command = command }));

            var parameter = JsonConvert.SerializeObject(new RemoteParameter() { Command = command });

            string response = await _bluetoothService.GetAsync(parameter);

            if (response == null)
                return default(T);

            if (typeof(T) == response.GetType())
            {
                return (T)(object)response;
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        public void Command(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return;
            }

            //_socketService.SendMessage(_connectedRemoteSystem.EndPoint, JsonConvert.SerializeObject(new RemoteParameter() { Command = command }));
        }

        internal async Task<bool> ConnectAsync(RomeRemoteSystem selectedRemoteSystem)
        {
            //_socketService.StopBroadcasting();
            _connectedRemoteSystem = selectedRemoteSystem;
            return await _bluetoothService.ConnectAsync(selectedRemoteSystem);
            //Command(RemoteCommands.MiriotConnect);
        }

        public async void Discover()
        {
            try
            {
                await _bluetoothService.InitializeAsync();

                _bluetoothService.Discovered = (system) =>
                {
                    var remoteSystem = _remoteSystems.FirstOrDefault(r => r.Id == system.Id);

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
            _bluetoothService.CommandReceived = CommandReceived;
            Task.Run(async () => await _bluetoothService.InitializeAsync());
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
                        _vm.HasNoConfiguration = false;
                        _vm.IsConfiguring = true;
                    });
                    return string.Empty;
                case RemoteCommands.LoadUser:
                    _dispatcherService.Invoke(async () =>
                    {
                        await _vm.LoadUser(_vm.User);
                    });
                    return string.Empty;
                case RemoteCommands.GetUser:
                    _dispatcherService.Invoke(() =>
                    {
                        _vm.HasNoConfiguration = false;
                        _vm.IsConfiguring = true;
                    });
                    return _vm.User.Id.ToString();
                case RemoteCommands.GetMiriotId:
                    return _platformService.GetSystemIdentifier();
                case RemoteCommands.GraphService_Initialize:
                    Messenger.Default.Send(new GraphServiceMessage(false));
                    return null;
                case RemoteCommands.GraphService_GetUser:
                    Messenger.Default.Send(new GraphServiceMessage(true));

                    var _graphService = SimpleIoc.Default.GetInstance<IGraphService>();

                    await _graphService.LoginAsync();
                    var graphUser = await _graphService.GetUserAsync();

                    return JsonConvert.SerializeObject(graphUser);
                case RemoteCommands.GoToCameraPage:
                    SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo(PageKeys.CameraSettings);
                    return null;
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

        internal void Stop()
        {
            _bluetoothService.StopAdv();
        }
    }
}
