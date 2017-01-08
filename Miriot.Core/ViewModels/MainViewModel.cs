using GalaSoft.MvvmLight.Command;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.System.Profile;
using GalaSoft.MvvmLight.Views;
using Miriot.Core.Services;
using Windows.Media;
using Windows.Media.SpeechSynthesis;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.ProjectOxford.Face;
using Miriot.Core.Messages;

namespace Miriot.Core.ViewModels
{
    public class MainViewModel : CustomViewModel
    {
        #region Variables
        private readonly IFileService _fileService;
        private readonly IPlatformService _platformService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INavigationService _navigationService;
        private readonly IFaceService _faceService;
        private readonly IVisionService _visionService;
        private User _user;
        private ObservableCollection<Widget> _widgets;
        private bool _isInternetAvailable;
        private States _currentState;

        #endregion

        #region Commands
        public RelayCommand<ServiceResponse> UsersIdentifiedCommand { get; set; }
        public RelayCommand<string> SpeakCommand { get; set; }
        public RelayCommand<States> StateChangedCommand { get; set; }
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

        public ObservableCollection<Widget> Widgets
        {
            get { return _widgets; }
            set { Set(() => Widgets, ref _widgets, value); }
        }

        public bool IsInternetAvailable
        {
            get { return _isInternetAvailable; }
            private set { Set(ref _isInternetAvailable, value); }
        }

        public byte[] LastFrameShot { get; set; }

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

        private string _subTitle;

        public string SubTitle
        {
            get { return _subTitle; }
            set { Set(ref _subTitle, value); }
        }

        public MainViewModel(
            IFileService fileService,
            IPlatformService platformService,
            IDispatcherService dispatcherService,
            INavigationService navigationService,
            IFaceService faceService,
            IVisionService visionService)
        {
            _fileService = fileService;
            _platformService = platformService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _faceService = faceService;
            _visionService = visionService;

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
            UsersIdentifiedCommand = new RelayCommand<ServiceResponse>(OnUsersIdentified);
            StateChangedCommand = new RelayCommand<States>(OnStateChanged);
            ActionNavigateTo = new RelayCommand<string>(OnNavigateTo);
            ActionTurnOnRadio = new RelayCommand(OnRadio);
            ActionTurnOnTv = new RelayCommand(OnTv);
            SpeakCommand = new RelayCommand<string>(OnSpeak);
        }

        private async void OnUsersIdentified(ServiceResponse response)
        {
            User = response?.Users?.FirstOrDefault();

            // User has been identified
            if (User != null)
            {
                var user = User;
                await LoadUsers(user);
            }
            else
            {
                //await ContinueProcess(response);
            }

            IsLoading = false;
        }

        private async Task LoadUsers(User user)
        {
            if (user.UserData.Widgets == null)
            {
                // In case of the user has no widgets
                user.UserData.Widgets = new List<Widget> { new Widget { Type = WidgetType.Time } };
            }

            await LoadWidgets(user.UserData.Widgets);

            Messenger.Default.Send(new ListeningMessage());

            user.Emotion = await GetEmotionAsync(user.Picture, user.FaceRectangleTop, user.FaceRectangleLeft);
        }

        private async Task LoadWidgets(List<Widget> widgets)
        {
            Widgets = new ObservableCollection<Widget>();

            foreach (var widget in widgets)
            {
                Widgets.Add(widget);

                // Wait 300ms to create a better transition effect
                await Task.Delay(300);
            }
        }

        private async void OnSpeak(string text)
        {

        }

        private void OnStateChanged(States state)
        {
            CurrentState = state;
            IsLoading = false;
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
            return await _faceService.UpdatePerson(User, User.Picture);
        }

        public async Task<bool> CreateAsync()
        {
            return await _faceService.CreatePerson(User.Picture, User.Name);
        }

        /// <summary>
        /// Envoi la photo de l'utilisateur au service Azure pour identification
        /// </summary>
        /// <param name="uri">Uri locale de la photo</param>
        /// <returns>Réponse du service de type User (null si échec)</returns>
        public async Task<ServiceResponse> GetUsersAsync()
        {
            try
            {
                var users = await _faceService.GetUsers(LastFrameShot);

                return users;
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "RateLimitExceeded")
                {
                    SubTitle = ex.Message;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<ServiceResponse> IdentifyFaces(VideoFrame frame)
        {
            LastFrameShot = await _fileService.EncodedBytes(frame.SoftwareBitmap);

            //MOCKED
            //var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            //uri = p.Path + "/untitled.png";

            // Post photo to Azure 
            // Compare faces & return identified user
            return await GetUsersAsync();
        }


        public async Task<UserEmotion> GetEmotionAsync(byte[] bitmap, int top, int left)
        {
            try
            {
                var emotion = await _faceService.GetEmotion(bitmap, top, left);

                return emotion;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetEmotionAsync: {ex.Message}");
            }

            return UserEmotion.Uknown;
        }

        public async Task<bool> IsToothbrushing()
        {
            try
            {
                var scene = await _visionService.CreateSceneAsync(User.Picture);

                return scene.IsToothbrushing;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public override void Cleanup()
        {
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;

            base.Cleanup();
        }

    }
}
