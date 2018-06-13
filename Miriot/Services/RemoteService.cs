using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Common.Model.Widgets;
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

		public Task<T> GetAsync<T>(RemoteCommands command, T parameter)
		{
			return CommandAsync<T>(new RemoteParameter { Command = command, SerializedData = JsonConvert.SerializeObject(parameter) });
		}

		public Task<T> GetAsync<T>(RemoteCommands command)
		{
			return CommandAsync<T>(new RemoteParameter { Command = command });
		}

		// Rename to Get
		public async Task<T> CommandAsync<T>(RemoteParameter parameter)
		{
			if (_connectedRemoteSystem == null)
			{
				Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
				return default(T);
			}

			string response = await _bluetoothService.GetAsync(JsonConvert.SerializeObject(parameter));

			if (response == null)
				return default(T);

			if (typeof(T) == response.GetType())
			{
				return (T)(object)response;
			}

			return JsonConvert.DeserializeObject<T>(response);
		}

		public Task SendAsync<T>(RemoteCommands command, T parameter)
		{
			return SendAsync(new RemoteParameter { Command = command, SerializedData = JsonConvert.SerializeObject(parameter) });
		}

		public Task SendAsync(RemoteCommands command)
		{
			return SendAsync(new RemoteParameter { Command = command });
		}

		public Task SendAsync(RemoteParameter parameter)
		{
			if (_connectedRemoteSystem == null)
			{
				Debug.WriteLine("You are using CommandAsync() but you are not connected to a remote system");
				return Task.FromResult(false);
			}

			return _bluetoothService.SendAsync(JsonConvert.SerializeObject(parameter));
		}

		internal Task<bool> ConnectAsync(RomeRemoteSystem selectedRemoteSystem)
		{
			_connectedRemoteSystem = selectedRemoteSystem;
			return _bluetoothService.ConnectAsync(selectedRemoteSystem);
		}

		public async void Discover()
		{
			try
			{
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

				await _bluetoothService.InitializeAsync();
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
					if (_vm.User != null)
						_dispatcherService.Invoke(() =>
						{
							_vm.HasNoConfiguration = false;
							_vm.IsConfiguring = true;
						});
					return _vm.User.Id.ToString();
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
				case RemoteCommands.DeezerService_GetUser:
					var deezerCode = JsonConvert.DeserializeObject<DeezerUser>(parameter.SerializedData);
					var service = SimpleIoc.Default.GetInstance<IOAuthService>();
					string token = await service.FinalizeAuthenticationAsync(deezerCode.Code);
					var user = await service.GetUserAsync(token);

					return JsonConvert.SerializeObject(deezerCode);
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
