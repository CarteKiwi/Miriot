using System;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Miriot.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Miriot.Services;

namespace Miriot
{
    public class ConnectViewModel : CustomViewModel
    {
        public ObservableCollection<RomeRemoteSystem> RemoteSystems { get; set; }

        private IRomeService _romeService;
        private readonly IDispatcherService _dispatcherService;

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
    }
}
