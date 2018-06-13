﻿using GalaSoft.MvvmLight.Command;
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
	public class ConnectViewModel : CustomViewModel
	{
		private readonly IDispatcherService _dispatcherService;
		private readonly INavigationService _navigationService;
		private readonly RemoteService _remoteService;
		private readonly MiriotService _miriotService;
		private RomeRemoteSystem _selectedSystem;
		private Timer _timer;
		private string _message;
		private string _messageTimeout;
		private bool _hasTimedOut;
		private RelayCommand _connectCommand;
		private RelayCommand<RomeRemoteSystem> _selectCommand;

		public bool HasTimedOut
		{
			get { return _hasTimedOut; }
			set { Set(ref _hasTimedOut, value); }
		}

		public bool HasAtLeastOneRemoteSystem => RemoteSystems.Any();

		public string Message
		{
			get { return _message; }
			set { Set(ref _message, value); }
		}

		public string MessageTimeout
		{
			get { return _messageTimeout; }
			set { Set(ref _messageTimeout, value); }
		}

		public RomeRemoteSystem SelectedRemoteSystem
		{
			get { return _selectedSystem; }
			set { Set(ref _selectedSystem, value); }
		}

		public RelayCommand<RomeRemoteSystem> SelectCommand
		{
			get
			{
				if (_selectCommand == null) _selectCommand = new RelayCommand<RomeRemoteSystem>(OnSelect);
				return _selectCommand;
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

		public ObservableCollection<RomeRemoteSystem> RemoteSystems { get; set; }

		public ConnectViewModel(
			IDispatcherService dispatcherService,
			INavigationService navigationService,
			RemoteService remoteService,
			MiriotService miriotService) : base(navigationService)
		{
			_dispatcherService = dispatcherService;
			_navigationService = navigationService;
			_remoteService = remoteService;
			_miriotService = miriotService;
			_timer = new Timer(OnTimeout, null, 10000, Timeout.Infinite);

			RemoteSystems = new ObservableCollection<RomeRemoteSystem>();
		}

		private void OnTimeout(object state)
		{
			if (RemoteSystems.Any())
				return;

			HasTimedOut = true;
			Message = Strings.ConnectionTimeout;
			MessageTimeout = Strings.CheckConnection;
		}

		protected override async Task InitializeAsync()
		{
			_remoteService.Added = OnAdded;
			_remoteService.Discover();
		}

		private void OnAdded(RomeRemoteSystem obj)
		{
			_dispatcherService.Invoke(() =>
			{
				RemoteSystems.Add(obj);
				RaisePropertyChanged(nameof(HasAtLeastOneRemoteSystem));
			});
		}

		private void OnRemoved(RomeRemoteSystem obj)
		{
			_dispatcherService.Invoke(() =>
			{
				RemoteSystems.Remove(obj);
				RaisePropertyChanged(nameof(HasAtLeastOneRemoteSystem));
			});
		}

		private void OnSelect(RomeRemoteSystem sys)
		{
			SelectedRemoteSystem = sys;
		}
		private void RunOnUiThread(System.Action action)
		{
			_dispatcherService.Invoke(action);
		}

		private async void OnConnect()
		{
			Message = Strings.Connecting;

			var success = await _remoteService.ConnectAsync(SelectedRemoteSystem);

			if (success)
			{
				Message = "Identification...";

				//await Task.Run(async () =>
				//{
				var userId = await _remoteService.GetAsync<string>(RemoteCommands.GetUser);

				Message = "Identification du miroir...";

				var miriotId = await _remoteService.GetAsync<string>(RemoteCommands.GetMiriotId);

				if (Guid.TryParse(userId, out Guid userGuid) && userGuid != Guid.Empty)
				{

					Message = Strings.RetrievingUser;

					var user = await _miriotService.GetUser(userGuid);

					RunOnUiThread(() =>
					{
						if (user != null && user.Id != Guid.Empty)
						{
							Message = string.Empty;
							_navigationService.NavigateTo(PageKeys.Profile, new MiriotParameter
							{
								User = user,
								Id = miriotId
							});
						}
						else
							Message = "Unregistered user";
					});
				}
				else
				{
					RunOnUiThread(() =>
					{
						_navigationService.NavigateTo(PageKeys.CameraSettings);
					});
				}
				//});
			}
			else
			{
				Message = Strings.ConnectionFailed;
			}
		}
	}
}
