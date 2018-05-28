using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Model;
using Miriot.Resources;
using Miriot.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels
{
    public class MainViewModel : CustomViewModel
    {
        #region Variables
        private readonly IFileService _fileService;
        private readonly IPlatformService _platformService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INavigationService _navigationService;
        private readonly IFrameAnalyzer<ServiceResponse> _frameService;
        private readonly IFaceService _faceService;
        private readonly IVisionService _visionService;
        private readonly ISpeechService _speechService;
        private readonly IGraphService _graphService;
        private readonly ILuisService _luisService;
        private readonly RemoteService _remoteService;
        private readonly MiriotService _miriotService;
        private string _title;
        private string _subTitle;
        private bool _isListeningYesNo;
        private User _user;
        private ObservableCollection<WidgetModel> _widgets;
        private bool _isConnectedToMobile;
        private bool _isInternetAvailable;
        private States _currentState;
        private CancellationTokenSource _cancellationToken;
        private byte[] _lastFrameShot;
        private Timer _toothbrushingLauncher;
        private Stopwatch _toothbrushingTimer;
        private SemaphoreSlim _toothbrushingSemaphore;
        private bool _isToothbrushing;
        private bool _isListening;
        private object _speakStream;
        private bool _hasNoConfiguration;
        private bool _isConfiguring;
        private bool _isPreviewing;
        #endregion

        #region Commands
        public RelayCommand<ServiceResponse> UsersIdentifiedCommand { get; private set; }
        public RelayCommand<string> ProceedSpeechCommand { get; private set; }
        public RelayCommand<States> StateChangedCommand { get; private set; }
        public RelayCommand<string> ActionNavigateTo { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand ToggleLedsCommand { get; private set; }
        #endregion

        public Action<LuisResponse> ActionCallback;

        public object SpeakStream
        {
            get => _speakStream;
            private set
            {
                Set(() => SpeakStream, ref _speakStream, value);
            }
        }

        public bool IsListening
        {
            get => _isListening;
            private set
            {
                _speechService.IsLimited = !value;

                Set(() => IsListening, ref _isListening, value);
            }
        }

        public User User
        {
            get => _user;
            private set
            {
                Set(() => User, ref _user, value);
                RaisePropertyChanged(() => IsConnected);
            }
        }

        public ObservableCollection<WidgetModel> Widgets
        {
            get => _widgets;
            private set => Set(() => Widgets, ref _widgets, value);
        }

        public bool IsConnectedToMobile
        {
            get => _isConnectedToMobile;
            private set => Set(ref _isConnectedToMobile, value);
        }

        public bool IsInternetAvailable
        {
            get => _isInternetAvailable;
            private set => Set(ref _isInternetAvailable, value);
        }

        public bool IsToothbrushing
        {
            get => _isToothbrushing;
            private set => Set(ref _isToothbrushing, value);
        }

        public bool IsConnected => User != null;

        public bool IsMobile => true;//AnalyticsInfo.VersionInfo.DeviceFamily.Contains("Mobile");

        public States CurrentState
        {
            get => _currentState;
            private set
            {
                // Force Active state for Mobile apps
                if (IsMobile)
                    value = States.Active;

                Set(ref _currentState, value);
            }
        }

        public bool IsListeningFirstName { get; private set; }

        public string Title
        {
            get => _title;
            private set => Set(ref _title, value);
        }

        public string SubTitle
        {
            get => _subTitle;
            set => Set(ref _subTitle, value);
        }

        public bool HasNoConfiguration
        {
            get => _hasNoConfiguration;
            set => Set(ref _hasNoConfiguration, value);
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

        public bool IsPreviewing
        {
            get => _isPreviewing;
            set
            {
                _isPreviewing = value;
                RaisePropertyChanged(() => IsPreviewing);
            }
        }

        public MainViewModel(
            IFileService fileService,
            IPlatformService platformService,
            IDispatcherService dispatcherService,
            INavigationService navigationService,
            IFrameAnalyzer<ServiceResponse> frameService,
            IFaceService faceService,
            IVisionService visionService,
            ISpeechService speechService,
            IGraphService graphService,
            ILuisService luisService,
            RemoteService remoteService,
            MiriotService miriotService) : base(navigationService)
        {
            _luisService = luisService;
            _remoteService = remoteService;
            _miriotService = miriotService;
            _fileService = fileService;
            _platformService = platformService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _frameService = frameService;
            _faceService = faceService;
            _visionService = visionService;
            _speechService = speechService;
            _graphService = graphService;

            _remoteService.Attach(this);

            SetCommands();
        }

        protected override async Task InitializeAsync()
        {
            Widgets = new ObservableCollection<WidgetModel>();
            _toothbrushingLauncher = new Timer(ToothbrushingLauncher);

            _toothbrushingSemaphore = new SemaphoreSlim(1);
            _toothbrushingTimer = new Stopwatch();

            _cancellationToken = new CancellationTokenSource();

            NetworkChange.NetworkAvailabilityChanged += OnNetworkStatusChanged;
            IsInternetAvailable = _platformService.IsInternetAvailable;

            await _speechService.InitializeAsync();
            _speechService.SetCommand(ProceedSpeechCommand);

            Messenger.Default.Register<DeviceConnectedMessage>(this, OnDeviceConnected);

            _remoteService.Listen();
        }

        private void OnDeviceConnected(DeviceConnectedMessage obj)
        {
            _dispatcherService.Invoke(() =>
            {
                IsConnectedToMobile = true;
                SubTitle = "Connecté avec " + obj.Name;
            });
        }

        private void OnNetworkStatusChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _dispatcherService.Invoke(() =>
            {
                IsInternetAvailable = _platformService.IsInternetAvailable;
            });
        }

        private void SetCommands()
        {
            ProceedSpeechCommand = new RelayCommand<string>(txt => _dispatcherService.Invoke(() => OnProceedSpeech(txt)));
            UsersIdentifiedCommand = new RelayCommand<ServiceResponse>(OnUsersIdentified);
            StateChangedCommand = new RelayCommand<States>(OnStateChanged);
            ActionNavigateTo = new RelayCommand<string>(OnNavigateTo);
            ResetCommand = new RelayCommand(OnReset);
        }

        private void OnReset()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
            Widgets?.Clear();
            IsLoading = false;
            HasNoConfiguration = false;

            //_toothbrushingLauncher.Change(0, Timeout.Infinite);
            IsListeningFirstName = false;
            _isListeningYesNo = false;
            User = null;
            Title = null;
            SubTitle = null;

            _speechService.Stop();
        }

        private string CleanForDemo(string text)
        {
            if (text.Contains("cineaction"))
                return text.Replace("cineaction", "synapson");
            else if (text.Contains("cynapium"))
                return text.Replace("cynapium", "synapson");
            else if (text.Contains("synapse"))
                return text.Replace("synapse", "synapson");

            return text;
        }

        private async Task CreateProfile(User user)
        {
            //Create profile
            var isSuccess = await CreateAsync(user);

            if (isSuccess)
            {
                SetMessage($"Très bien. Ravi de faire votre connaissance {user.Name}.", "Utilisez votre téléphone pour ajouter des widgets");
                Speak($"Très bien. Ravi de faire votre connaissance {user.Name}.");
            }
            else
            {
                SetMessage(null, Strings.UnableToSaveAccount);
            }
        }

        private async void OnProceedSpeech(string text)
        {
            await _speechService.StartListeningAsync();

            //if (!_isListeningYesNo && !IsListeningFirstName && !text.Contains("miriot"))
            //{
            //    return;
            //}

            text = CleanForDemo(text);

            if (_isListeningYesNo)
            {
                _isListeningYesNo = false;

                if (text.Contains("oui"))
                {
                    IsListeningFirstName = false;

                    await CreateProfile(User);
                }
                else if (text.Contains("non"))
                {
                    RepeatPromptForUnknownFace();
                }
                else
                {
                    SetMessage(string.Empty, "Répondez par oui ou par non");
                }
            }
            else
            {
                if (text.Contains("Miriot") || text.Contains("Myriade") || text.Contains("Mariotte") || text.Contains("Mireille hot"))
                {
                    IsListening = true;
                    SetMessage("J'écoute.", "vous pouvez parler.");

                    return;
                }

                if (!IsListening)
                    return;

                SetMessage("Un instant...", "je réfléchis...");

                IsLoading = true;

                IsListening = false;

                // Appel au service LUIS
                var luisResponse = await _luisService.AskLuisAsync(text);

                // Récupération de la première action (intent avec Score le plus élevé)
                var intent = luisResponse?.TopScoringIntent;

                if (IsListeningFirstName)
                {
                    if (intent != null && intent.Intent == "CreateProfile")
                        SetNewProfileMessage(luisResponse.Entities.OrderByDescending(e => e.Score).FirstOrDefault());
                    else
                        RepeatPromptForUnknownFace();
                }
                else
                {
                    if (intent != null)
                    {
                        ActionCallback.Invoke(luisResponse);
                    }

                    SetMessage(string.Empty, string.Empty);
                }
            }

            IsLoading = false;
        }

        public void Repeat()
        {
            RepeatPromptForUnknownFace();
        }

        private void SetNewProfileMessage(LuisEntity entity)
        {
            var name = entity.Entity;

            User = new User { Name = name, Picture = _lastFrameShot };

            SetMessage($"Bonjour {name.ToUpperInvariant()}", "Aie-je bien entendu votre prénom ?");
            Speak($"Bonjour {name.ToUpperInvariant()}. Aie-je bien entendu votre prénom ?");

            _isListeningYesNo = true;
        }

        private async void ToothbrushingLauncher(object sender)
        {
            if (!_toothbrushingSemaphore.Wait(0))
                return;

            var scene = await GetToothbrushingSceneAsync();

            IsToothbrushing = scene.IsToothbrushing;

            if (!IsToothbrushing)
            {
                ToothbrushingEnding();
            }
            else
            {
                ToothbrushingStarting();
            }

            _toothbrushingSemaphore.Release();
        }

        private void ToothbrushingStarting()
        {
            if (!_toothbrushingTimer.IsRunning)
                _toothbrushingTimer.Start();
        }

        private void ToothbrushingEnding()
        {
            _toothbrushingTimer.Stop();

            if (_toothbrushingTimer.Elapsed.Seconds > 5)
            {
                if (User.ToothbrushingHistory == null)
                    User.ToothbrushingHistory = new List<ToothbrushingEntry>();

                User.ToothbrushingHistory.Add(new ToothbrushingEntry() { Date = DateTime.UtcNow, Duration = _toothbrushingTimer.Elapsed.Seconds });
            }
        }

        private async void OnUsersIdentified(ServiceResponse response)
        {
            User = response?.Users?.FirstOrDefault();

            // User has been identified
            if (User != null)
            {
                var user = User;
                try
                {
                    await LoadUser(user);

                    if (!await UpdateUser(user))
                    {
                        SubTitle = Strings.UnableToUpdateAccount;
                    }
#if MOCK
                    //_remoteService.Listen();
                    //_toothbrushingLauncher.Change(0, Timeout.Infinite);
#else
                    //_toothbrushingLauncher.Change(0, 3);
#endif
                }
                catch (TaskCanceledException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                ContinueProcess(response);
            }

            IsLoading = false;
        }

        public async Task LoadUser(User user)
        {
            var u = await _miriotService.GetUser(user.Id);

            if (u != null)
                user = u;

            var sysId = _platformService.GetSystemIdentifier();

            if (user.Devices == null)
            {
                user.Devices = new List<MiriotConfiguration>();
            }

            var config = user.Devices.FirstOrDefault(c => c.MiriotDeviceId == sysId);

            // No config for this mirror
            if (config == null)
            {
                HasNoConfiguration = true;
            }
            else
            {
                HasNoConfiguration = false;
                await LoadWidgets(config.Widgets);
            }

            await _speechService.StartListeningAsync();

            //user.Emotion = await GetEmotionAsync(user.Picture, user.FaceRectangle.Top, user.FaceRectangle.Left);
            user.Picture = User != null ? User.Picture : user.Picture;
            User = user;
        }

        public async Task<bool> UpdateUser(User user)
        {
            user.LastLoginDate = DateTime.UtcNow;

            RaisePropertyChanged(() => user.LastLoginDate);

            await UpdatePersonAsync(user);

            return await _miriotService.UpdateUserAsync(user);
        }

        private async Task LoadWidgets(IEnumerable<Widget> widgets)
        {
            Widgets.Clear();

            foreach (var widget in widgets)
            {
                Widgets.Add(widget.ToModel());

                // Wait 100ms to create a better transition effect
                await Task.Delay(100, _cancellationToken.Token);
            }

            if (CurrentState == States.Inactive)
            {
                Widgets.Clear();
            }

            //ActionCallback(new IntentResponse()
            //{
            //    Actions = new List<Common.Action>() {
            //        new Common.Action() {
            //            Triggered = true,
            //            Name = "Search",
            //            Parameters = new List<Common.Parameter>() {
            //                new Common.Parameter() {
            //                    Value = new List<ParameterValue>() {
            //                        new ParameterValue() { Entity = "Michael Jackson" }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //});
        }

        private void ContinueProcess(ServiceResponse response)
        {
            if (response?.Error != null)
            {
                if (response.Error.Value == ErrorType.NoFaceDetected)
                {
                    Title = "Je n'y vois pas très clair...";
                    SubTitle = "Aucun visage n'a été identifé.";
                }
                else if (response.Error.Value == ErrorType.UnknownFace)
                {
                    PromptForUnknownFace();
                }
                else
                {
                    SetWelcomeMessage(null);
                }
            }
        }

        private async void PromptForUnknownFace()
        {
            SetMessage("Bonjour. Je m'appelle MirioT.", "Quel est votre prénom ? (dites: je m'appelle...)");
            Speak("Bonjour, je m'appelle Miriotte. Et vous ? Quel est votre prénom ?");

            IsListeningFirstName = true;

            await _speechService.StartListeningAsync();
        }

        private async void RepeatPromptForUnknownFace()
        {
            SetMessage("Je n'ai pas compris", "Quel est votre prénom ? (dites: je m'appelle...)");
            Speak("Je n'ai pas compris. Quel est votre prénom ?");

            IsListeningFirstName = true;

            await _speechService.StartListeningAsync();
        }

        private void SetWelcomeMessage(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                SetMessage(string.Empty, "Vérifiez votre connexion internet");
                return;
            }

            Title = $"Bonjour {User.Name}";
        }

        private void SetMessage(string title, string subTitle)
        {
            Title = title;
            SubTitle = subTitle;
        }

        private async void Speak(string text)
        {
            SpeakStream = await _speechService.SynthesizeTextToStreamAsync(text);
        }

        private void OnStateChanged(States state)
        {
            if (state == States.Inactive)
            {
                OnReset();
            }

            CurrentState = state;
        }

        private void OnNavigateTo(string pageKey)
        {
#if MOCK
            _remoteService.Listen();
#else
            _remoteService.Listen();
            //_navigationService.NavigateTo(PageKeys.CameraSettings);
#endif
        }

        public async Task<bool> UpdatePersonAsync(User user)
        {
            return await _faceService.UpdatePerson(user, user.Picture);
        }

        private async Task<bool> CreateAsync(User user)
        {
            if (await _faceService.CreatePerson(user.Picture, user.Name))
            {
                return await _miriotService.CreateUser(user);
            }

            return false;
        }

        /// <summary>
        /// Envoi la photo de l'utilisateur au service Oxford pour identification
        /// </summary>
        /// <returns>Réponse du service de type User (null si échec)</returns>
        private async Task<ServiceResponse> GetUsersAsync()
        {
            try
            {
                var users = await _faceService.GetUsers(_lastFrameShot);

                return users;
            }
            //catch (FaceAPIException ex)
            //{
            //    if (ex.ErrorCode == "RateLimitExceeded")
            //    {
            //        Debug.WriteLine($"GetUserAsync: RateLimitExceeded: {ex.Message}");
            //    }
            //}
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<ServiceResponse> IdentifyFaces(byte[] bitmap)
        {
            _lastFrameShot = bitmap;

            // Post photo to Azure 
            // Compare faces & return identified user
            return await GetUsersAsync();
        }

        private async Task<UserEmotion> GetEmotionAsync(byte[] bitmap, int top, int left)
        {
            return await _faceService.GetEmotion(bitmap, top, left);
        }

        private async Task<Scene> GetToothbrushingSceneAsync()
        {
            try
            {
                Debug.WriteLine("Toothbrushing checking...");

                byte[] bitmap = await _frameService.GetFrame();

                if (bitmap == null)
                    return new Scene();

                var scene = await _visionService.CreateSceneAsync(bitmap);

                Debug.WriteLine($"Toothbrushing checked : {scene.IsToothbrushing}");

                return scene;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new Scene();
            }
        }

        public override void Cleanup()
        {
            _remoteService.Stop();
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkStatusChanged;
            OnReset();
            base.Cleanup();
        }
    }
}
