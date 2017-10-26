using System;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Miriot.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Miriot.ViewModels
{
    public class ConnectViewModel : CustomViewModel
    {
        public ObservableCollection<RomeRemoteSystem> RemoteSystems { get; set; }

        private IRomeService _romeService;

        public ConnectViewModel(IRomeService romeService)
        {
            _romeService = romeService;
        }

        public void Initialize()
        {
            RemoteSystems = new ObservableCollection<RomeRemoteSystem>();
            _romeService.Added = Refresh;
            _romeService.InitializeAsync();
        }

        private void Refresh(RomeRemoteSystem obj)
        {
            RemoteSystems.Add(obj);
        }
    }
}
