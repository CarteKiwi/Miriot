using GalaSoft.MvvmLight.Command;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.Helpers;
using Miriot.Core.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.System.Profile;

namespace Miriot.Core.ViewModels
{
    public class MainViewModel : CustomViewModel
    {
        #region Variables
        private readonly IFileService _fileService;
        private readonly IPlatformService _platformService;
        private readonly IDispatcherService _dispatcherService;
        private readonly FaceHelper _faceHelper;
        private User _user;
        private ObservableCollection<IWidgetBase> _widgets;
        private bool _isInternetAvailable;
        private States _currentState;
        private bool _hasSensor = true;
        #endregion

        #region Commands
        public RelayCommand ActionTurnOnTv { get; set; }
        public RelayCommand ActionTurnOnRadio { get; set; }
        #endregion

        public User User
        {
            get { return _user; }
            set
            {
                Set(() => User, ref _user, value);
                RaisePropertyChanged(() => IsConnected);
            }
        }

        public ObservableCollection<IWidgetBase> Widgets
        {
            get { return _widgets; }
            set { Set(() => Widgets, ref _widgets, value); }
        }

        public bool IsInternetAvailable
        {
            get { return _isInternetAvailable; }
            private set { Set(ref _isInternetAvailable, value); }
        }

        public bool IsConnected => User != null;

        public bool IsMobile => AnalyticsInfo.VersionInfo.DeviceFamily.Contains("Mobile");

        public bool HasSensor
        {
            get { return _hasSensor; }
            set { Set(ref _hasSensor, value); }
        }


        public States CurrentState
        {
            get { return _currentState; }
            set
            {
                // Force Active state for Mobile apps
                if (IsMobile || !HasSensor)
                    value = States.Active;

                Set(ref _currentState, value);
            }
        }

        public MainViewModel(
            IFileService fileService,
            IPlatformService platformService,
            IDispatcherService dispatcherService)
        {
            _fileService = fileService;
            _platformService = platformService;
            _dispatcherService = dispatcherService;
            _faceHelper = new FaceHelper(fileService);

            SetCommands();

            IsInternetAvailable = _platformService.IsInternetAvailable;

            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        }

        private void OnNetworkStatusChanged(object sender)
        {
            _dispatcherService.Invoke(() =>
            {
                IsInternetAvailable = _platformService.IsInternetAvailable;
            });
        }

        private void SetCommands()
        {
            ActionTurnOnRadio = new RelayCommand(OnRadio);
            ActionTurnOnTv = new RelayCommand(OnTv);
        }

        private void OnTv()
        {
            throw new NotImplementedException();
        }

        private void OnRadio()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateUserAsync()
        {
            return await _faceHelper.UpdatePerson(User, _fileService.GetBytes(User.PictureLocalPath));
        }

        public async Task<bool> CreateAsync(string name, string filePath)
        {
            return await _faceHelper.CreatePerson(filePath, name);
        }

        /// <summary>
        /// Envoi la photo de l'utilisateur au service Azure pour identification
        /// </summary>
        /// <param name="uri">Uri locale de la photo</param>
        /// <returns>Réponse du service de type User (null si échec)</returns>
        public async Task<ServiceResponse> GetUserAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;

            try
            {
                var user = await _faceHelper.GetUser(uri);

                return user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserAsync: {ex.Message}");
            }

            return null;
        }

        public override void Cleanup()
        {
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;

            base.Cleanup();
        }

    }
}
