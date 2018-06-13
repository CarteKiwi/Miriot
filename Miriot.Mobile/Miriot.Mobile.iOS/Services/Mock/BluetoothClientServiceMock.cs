using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Model;
using Miriot.Services;
using Newtonsoft.Json;
using Plugin.BluetoothLE;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.iOS.Services
{
	public class BluetoothClientServiceMock : IBluetoothService
	{
		private const int ConnectionTimeout = 30000;

		public Func<RemoteParameter, Task<string>> CommandReceived { get; set; }
		public Action<RomeRemoteSystem> Discovered { get; set; }

		private IDevice _connectedDevice;
		private User _user;

		private void Scan()
		{
			Discovered?.Invoke(new RomeRemoteSystem(null)
			{
				DisplayName = "Miriot Mock",
				Id = "1"
			});

		}

		private void StopScan()
		{

		}

		public async Task<bool> ConnectAsync(RomeRemoteSystem system)
		{
			try
			{
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to connect : " + ex.Message);
				return false;
			}
		}

		public Task InitializeAsync()
		{
			CommandReceived = OnCommandReceived;
			Scan();

			return Task.FromResult(true);
		}

		private async Task<string> OnCommandReceived(RemoteParameter parameter)
		{
			switch (parameter.Command)
			{
				case RemoteCommands.MiriotConfiguring:
					return string.Empty;
				case RemoteCommands.LoadUser:

					//await LoadUser(new User() { Id = Guid.Parse("") });

					return string.Empty;
				case RemoteCommands.GetUser:

					return "64e4d01e-d52f-4c1a-8bff-e6cef6f505c3";
				case RemoteCommands.GetMiriotId:
					return "1";
				case RemoteCommands.GraphService_Initialize:
					return null;
				case RemoteCommands.GraphService_GetUser:
					var u = new GraphUser { Name = "Guillaume Mock", Photo = null };
					return JsonConvert.SerializeObject(u);
				case RemoteCommands.GoToCameraPage:
					return null;
				default:
					return null;
			}
		}

		public async Task<string> GetAsync(string value)
		{
			try
			{
				var parameter = JsonConvert.DeserializeObject<RemoteParameter>(value);

				return await CommandReceived(parameter);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("GetAsync failed: " + ex.Message);
				return string.Empty;
			}
		}

		public async Task SendAsync(string value)
		{
			try
			{
				var parameter = JsonConvert.DeserializeObject<RemoteParameter>(value);
				await CommandReceived(parameter);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("SendAsync failed: " + ex.Message);
			}
		}

		public void StopAdv()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}
	}
}