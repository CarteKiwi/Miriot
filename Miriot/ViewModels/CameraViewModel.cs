using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Model;
using Miriot.Resources;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels
{
	public class CameraViewModel : CustomViewModel
	{
		private readonly IDispatcherService _dispatcherService;
		private readonly INavigationService _navigationService;
		private readonly RemoteService _remoteService;
		private readonly MiriotService _miriotService;
		private RelayCommand _connectCommand;
		private RelayCommand _cancelCommand;
		private RelayCommand<double> _adjustCommand;
		private double _brightness;

		public double Brightness
		{
			get { return _brightness; }
			set
			{
				_brightness = value;
			}
		}


		public RelayCommand<double> AdjustCommand
		{
			get
			{
				if (_adjustCommand == null) _adjustCommand = new RelayCommand<double>(OnAdjust);
				return _adjustCommand;
			}
		}

		public RelayCommand ConnectCommand
		{
			get
			{
				if (_connectCommand == null) _connectCommand = new RelayCommand(OnConnect);
				return _connectCommand;
			}
		}

		public RelayCommand CancelCommand
		{
			get
			{
				if (_cancelCommand == null) _cancelCommand = new RelayCommand(OnCancel);
				return _cancelCommand;
			}
		}


		public CameraViewModel(
			IDispatcherService dispatcherService,
			INavigationService navigationService,
			RemoteService remoteService,
			MiriotService miriotService) : base(navigationService)
		{
			_dispatcherService = dispatcherService;
			_navigationService = navigationService;
			_remoteService = remoteService;
			_miriotService = miriotService;
		}


		protected override async Task InitializeAsync()
		{

		}

		private async void OnAdjust(double value)
		{
			await _remoteService.SendAsync(RemoteCommands.CameraAdjustBrightness, value);
		}

		private void RunOnUiThread(System.Action action)
		{
			_dispatcherService.Invoke(action);
		}

		private async void OnConnect()
		{
			await _remoteService.SendAsync(RemoteCommands.CameraPersist);
			_navigationService.NavigateTo(PageKeys.Main);
		}

		private void OnCancel()
		{
			_navigationService.NavigateTo(PageKeys.Main);
		}
	}
}
