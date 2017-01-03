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
using GalaSoft.MvvmLight.Views;

namespace Miriot.Core.ViewModels
{
    public class MainViewModel : CustomViewModel
    {
        #region Variables
        private readonly IFileService _fileService;
        private readonly IPlatformService _platformService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INavigationService _navigationService;
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
        public RelayCommand<string> ActionNavigateTo { get; set; }
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

        public States CurrentState
        {
            get { return _currentState; }
            set
            {
                // Force Active state for Mobile apps
                if (IsMobile)
                    value = States.Active;

                Set(ref _currentState, value);
            }
        }

        public MainViewModel(
            IFileService fileService,
            IPlatformService platformService,
            IDispatcherService dispatcherService,
            INavigationService navigationService)
        {
            _fileService = fileService;
            _platformService = platformService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
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
            ActionNavigateTo = new RelayCommand<string>(OnNavigateTo);
            ActionTurnOnRadio = new RelayCommand(OnRadio);
            ActionTurnOnTv = new RelayCommand(OnTv);
        }

        private void OnNavigateTo(string pageKey)
        {
            var user = User;
            _navigationService.NavigateTo(pageKey, user);
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
        public async Task<ServiceResponse> GetUsersAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;

            try
            {
                var users = await _faceHelper.GetUsers(uri);

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<UserEmotion> GetEmotionAsync(string uri, int top, int left)
        {
            if (string.IsNullOrEmpty(uri)) return UserEmotion.Uknown;

            try
            {
                var emotion = await _faceHelper.GetEmotion(uri, top, left);

                return emotion;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetEmotionAsync: {ex.Message}");
            }

            return UserEmotion.Uknown;
        }

        public override void Cleanup()
        {
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;

            base.Cleanup();
        }

    }
}
