using System;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Miriot.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Miriot.Services;
using GalaSoft.MvvmLight.Command;

namespace Miriot
{
    public class ConnectViewModel : CustomViewModel
    {
        public RomeRemoteSystem SelectedRemoteSystem
        {
            get
            {
                return _selectedSystem;
            }
            set { Set(ref _selectedSystem, value); }
        }

        private RelayCommand<RomeRemoteSystem> _selectCommand;
        public RelayCommand<RomeRemoteSystem> SelectCommand
        {
            get
            {
                if (_selectCommand == null) _selectCommand = new RelayCommand<RomeRemoteSystem>(OnSelect);
                return _selectCommand;
            }
        }

        private RelayCommand<string> _sendCommand;
        public RelayCommand<string> SendCommand
        {
            get
            {
                if (_sendCommand == null) _sendCommand = new RelayCommand<string>(OnCommand);
                return _sendCommand;
            }
        }

        public ObservableCollection<RomeRemoteSystem> RemoteSystems { get; set; }

        private IRomeService _romeService;
        private readonly IDispatcherService _dispatcherService;
        private RomeRemoteSystem _selectedSystem;

        public ConnectViewModel(IRomeService romeService, IDispatcherService dispatcherService)
        {
            _romeService = romeService;
            _dispatcherService = dispatcherService;
            RemoteSystems = new ObservableCollection<RomeRemoteSystem>();
        }

        public async Task InitializeAsync()
        {
            _romeService.Added = Refresh;
            await _romeService.InitializeAsync();
        }

        private void Refresh(RomeRemoteSystem obj)
        {
            _dispatcherService.Invoke(() =>
            {
                RemoteSystems.Add(obj);
            });
        }

        private void OnSelect(RomeRemoteSystem sys)
        {
            SelectedRemoteSystem = sys;
        }

        private void OnCommand(string obj)
        {
            _romeService.SendCommandAsync(SelectedRemoteSystem, obj);
        }
    }
}
