using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Miriot.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.ViewModels
{
    public class SettingsViewModel : CustomViewModel
    {
        #region Variables
        private readonly IFileService _fileService;
        private readonly IWifiService _wifiService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INavigationService _navigationService;
        private readonly IFrameAnalyzer<ServiceResponse> _frameService;
        private readonly IFaceService _faceService;
        private readonly RemoteService _remoteService;
        private readonly MiriotService _miriotService;
        private string _title;
        private string _subTitle;
        private bool _isInternetAvailable;
        private CancellationTokenSource _cancellationToken;
        private byte[] _lastFrameShot;
        private bool _isConfiguring;
        #endregion

        #region Commands
        public RelayCommand<States> StateChangedCommand { get; private set; }
        public RelayCommand<string> ActionNavigateTo { get; private set; }
        #endregion

        public ObservableCollection<WifiNetwork> Wifis { get; private set; }

        public bool IsInternetAvailable
        {
            get => _isInternetAvailable;
            private set => Set(ref _isInternetAvailable, value);
        }

        public string Title
        {
            get => _title;
            private set => Set(ref _title, value);
        }

        public string SubTitle
        {
            get => _subTitle;
            private set => Set(ref _subTitle, value);
        }

        public bool IsConfiguring
        {
            get => _isConfiguring;
            set
            {
                _isConfiguring = value;
                RaisePropertyChanged(() => IsConfiguring);
            }
        }

        public SettingsViewModel(
            IFileService fileService,
            IWifiService wifiService,
            IDispatcherService dispatcherService,
            INavigationService navigationService,
            IFrameAnalyzer<ServiceResponse> frameService,
            IFaceService faceService,
            RemoteService remoteService,
            MiriotService miriotService) : base(navigationService)
        {
            _remoteService = remoteService;
            _miriotService = miriotService;
            _fileService = fileService;
            _wifiService = wifiService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _frameService = frameService;
            _faceService = faceService;

            //_remoteService.Attach(this);
            _remoteService.Listen();

            Wifis = new ObservableCollection<WifiNetwork>();
        }

        protected override async Task InitializeAsync()
        {
            var wifis = await _wifiService.GetWifiAsync();

            foreach (var w in wifis)
            {
                if (Wifis.FirstOrDefault(wifi => wifi.Bssid == w.Bssid) == null)
                    Wifis.Add(w);
            }
        }

        private async Task ConnectWifiAsync(string bssid, string pwd)
        {
            await _wifiService.ConnectWifiAsync(bssid, pwd);
        }
    }
}
