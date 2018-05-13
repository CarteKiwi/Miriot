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

        // Rename to Get
        public async Task<T> CommandAsync<T>(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return default(T);
            }

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

        public async Task SendAsync(RemoteCommands command)
        {
            if (_connectedRemoteSystem == null)
            {
                Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
                return;
            }

            await _bluetoothService.SendAsync(JsonConvert.SerializeObject(new RemoteParameter { Command = command }));
        }

        internal async Task<bool> ConnectAsync(RomeRemoteSystem selectedRemoteSystem)
        {
            _connectedRemoteSystem = selectedRemoteSystem;
            return await _bluetoothService.ConnectAsync(selectedRemoteSystem);
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
                    if (_vm.User != null)
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
                    return _vm.User?.Id.ToString();
                case RemoteCommands.GetMiriotId:
                    return _platformService.GetSystemIdentifier();
                case RemoteCommands.GraphService_Initialize:
                    var _graphService = SimpleIoc.Default.GetInstance<IGraphService>();
                    string code = await _graphService.GetCodeAsync();

                    _dispatcherService.Invoke(() =>
                    {
                        _vm.SubTitle = "Code : " + code;
                    });

                    // Waiting until user has logged
                    if (await _graphService.LoginAsync())
                    {
                        var graphUser = await _graphService.GetUserAsync();

                        _dispatcherService.Invoke(async () =>
                        {
                            _vm.SubTitle = "Connecté en tant que " + graphUser.Name;
                            await _vm.LoadUser(_vm.User);
                        });

                        return JsonConvert.SerializeObject(graphUser);
                    }
                    else
                    {
                        _dispatcherService.Invoke(() =>
                        {
                            _vm.SubTitle = "L'authentification a échouée.";
                        });
                    }

                    return null;
                case RemoteCommands.GoToCameraPage:
                    _dispatcherService.Invoke(() =>
                    {
                        var ns = SimpleIoc.Default.GetInstance<INavigationService>();
                        ns.NavigateTo(PageKeys.CameraSettings);
                    });
                    return null;
                case RemoteCommands.CameraPreview:
                    _dispatcherService.Invoke(() =>
                    {
                        var showPreview = JsonConvert.DeserializeObject<bool>(parameter.SerializedData);
                        var camera = SimpleIoc.Default.GetInstance<ICameraService>();
                        camera.ShowPreview = showPreview;
                    });
                    return null;
                case RemoteCommands.CameraAdjustBrightness:
                    _dispatcherService.Invoke(() =>
                    {
                        var value = JsonConvert.DeserializeObject<double>(parameter.SerializedData);
                        var cameraS = SimpleIoc.Default.GetInstance<ICameraService>();
                        cameraS.AdjustBrightness(value);
                    });
                    return null;
                case RemoteCommands.CameraPersist:
                    var camera2 = SimpleIoc.Default.GetInstance<ICameraService>();
                    camera2.PersistSettings();
                    return null;
                default:
                    return null;
            }
        }

        internal void Stop()
        {
            _bluetoothService.StopAdv();
        }
    }
}
