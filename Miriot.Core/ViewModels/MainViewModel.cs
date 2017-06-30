using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.ProjectOxford.Face;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Core.Messages;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml;

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
        private string _title;
        private string _subTitle;
        private bool _isListeningYesNo;
        private User _user;
        private ObservableCollection<Widget> _widgets;
        private bool _isInternetAvailable;
        private States _currentState;
        private CancellationTokenSource _cancellationToken;
        private byte[] _lastFrameShot;
        private DispatcherTimer _toothbrushingLauncher;
        private Stopwatch _toothbrushingTimer;
        private SemaphoreSlim _toothbrushingSemaphore;
        private bool _isToothbrushing;
        private bool _isListening;
        #endregion

        #region Commands
        public RelayCommand<ServiceResponse> UsersIdentifiedCommand { get; private set; }
        public RelayCommand<string> ProceedSpeechCommand { get; private set; }
        public RelayCommand<States> StateChangedCommand { get; private set; }
        public RelayCommand<string> ActionNavigateTo { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        #endregion

        private IRandomAccessStream _speakStream;

        public IRandomAccessStream SpeakStream
        {
            get => _speakStream;
            set
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

        public ObservableCollection<Widget> Widgets
        {
            get => _widgets;
            private set => Set(() => Widgets, ref _widgets, value);
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

        public bool IsMobile => AnalyticsInfo.VersionInfo.DeviceFamily.Contains("Mobile");

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
            private set => Set(ref _subTitle, value);
        }

        public MainViewModel(
            IFileService fileService,
            IPlatformService platformService,
            IDispatcherService dispatcherService,
            INavigationService navigationService,
            IFrameAnalyzer<ServiceResponse> frameService,
            IFaceService faceService,
            IVisionService visionService,
            ISpeechService speechService)
        {
            _fileService = fileService;
            _platformService = platformService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _frameService = frameService;
            _faceService = faceService;
            _visionService = visionService;
            _speechService = speechService;

            SetCommands();
        }

        public void Initialize()
        {
            _toothbrushingLauncher = new DispatcherTimer { Interval = new TimeSpan(0, 0, 3) };
            _toothbrushingLauncher.Tick += ToothbrushingLauncher;
            _toothbrushingSemaphore = new SemaphoreSlim(1);
            _toothbrushingTimer = new Stopwatch();

            _cancellationToken = new CancellationTokenSource();

            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
            IsInternetAvailable = _platformService.IsInternetAvailable;

            _speechService.InitializeAsync();
            _speechService.SetCommand(ProceedSpeechCommand);
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
            ProceedSpeechCommand = new RelayCommand<string>(OnProceedSpeech);
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

            _toothbrushingLauncher.Stop();
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

        private async Task CreateProfile()
        {
            //Create profile
            var isSuccess = await CreateAsync();

            if (isSuccess)
            {
                SetMessage($"Très bien. Ravi de faire votre connaissance {User.Name}.", "Utilisez votre téléphone pour ajouter des widgets");
                Speak($"Très bien. Ravi de faire votre connaissance {User.Name}.");
            }
            else
            {
                SetMessage(null, "Impossible d'enregistrer le compte");
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

                    await CreateProfile();
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
                if (text.Contains("Miriot") || text.Contains("Myriade") || text.Contains("Mariotte"))
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
                var res = await AskLuisAsync(text);

                // Récupération de la première action (intent avec Score le plus élevé)
                var intent = res?.Intents?.OrderByDescending(e => e.Score).FirstOrDefault();

                if (IsListeningFirstName)
                {
                    if (intent != null && intent.Intent == "CreateProfile")
                        SetNewProfileMessage(intent);
                    else
                        RepeatPromptForUnknownFace();
                }
                else
                {
                    if (intent != null)
                    {
                        Messenger.Default.Send(new ActionMessage(intent));
                    }

                    SetMessage(string.Empty, string.Empty);
                }
            }

            IsLoading = false;
        }

        private async Task<LuisResponse> AskLuisAsync(string words)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.projectoxford.ai/luis/v1/");
                    var res = await client.GetAsync($"application?id=e2fa615c-2c3a-4f2d-a82b-5151223d4cca&subscription-key=48511b6fad9a4cb7acf6e3e583d95efd&q={words}");
                    var c = await res.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<LuisResponse>(c);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Repeat()
        {
            RepeatPromptForUnknownFace();
        }

        private void SetNewProfileMessage(IntentResponse intent)
        {
            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string name = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null && p.Name == "Firstname")
                        name = p.Value.OrderByDescending(e => e.Score).First().Entity;
                }

            User = new User { Name = name, Picture = _lastFrameShot };

            SetMessage($"Bonjour {name.ToUpperInvariant()}", "Aie-je bien entendu votre prénom ?");
            Speak($"Bonjour {name.ToUpperInvariant()}. Aie-je bien entendu votre prénom ?");

            _isListeningYesNo = true;
        }

        private async void ToothbrushingLauncher(object sender, object e)
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
            _toothbrushingLauncher.Interval = new TimeSpan(0, 0, 3);

            if (!_toothbrushingTimer.IsRunning)
                _toothbrushingTimer.Start();
        }

        private void ToothbrushingEnding()
        {
            _toothbrushingTimer.Stop();
            _toothbrushingLauncher.Interval = new TimeSpan(0, 0, 3);

            if (_toothbrushingTimer.Elapsed.Seconds > 5)
            {
                if (User.UserData.ToothbrushingHistory == null)
                    User.UserData.ToothbrushingHistory = new Dictionary<DateTime, int>();
                User.UserData.ToothbrushingHistory.Add(DateTime.UtcNow, _toothbrushingTimer.Elapsed.Seconds);
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
                    await UpdateUser(user);

                    _toothbrushingLauncher.Start();
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

        private async Task LoadUser(User user)
        {
            if (user.UserData.Widgets == null)
            {
                // In case of the user has no widgets
                user.UserData.Widgets = new List<Widget> { new Widget { Type = WidgetType.Time } };
            }

            await LoadWidgets(user.UserData.Widgets);

            await _speechService.StartListeningAsync();

            user.Emotion = await GetEmotionAsync(user.Picture, user.FaceRectangle.Top, user.FaceRectangle.Left);

            user.PreviousLoginDate = user.UserData.PreviousLoginDate;
        }

        private async Task UpdateUser(User user)
        {
            user.UserData.PreviousEmotion = user.Emotion;
            user.UserData.PreviousLoginDate = DateTime.UtcNow;

            await _faceService.UpdateUserDataAsync(user);
        }

        private async Task LoadWidgets(IEnumerable<Widget> widgets)
        {
            Widgets = new ObservableCollection<Widget>();

            foreach (var widget in widgets)
            {
                Widgets.Add(widget);

                // Wait 100ms to create a better transition effect
                await Task.Delay(100, _cancellationToken.Token);
            }

            if (CurrentState == States.Inactive)
            {
                Widgets.Clear();
            }
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
            var user = User;
            _navigationService.NavigateTo(pageKey, user);
        }

        public async Task<bool> UpdateUserAsync()
        {
            return await _faceService.UpdatePerson(User, User.Picture);
        }

        private async Task<bool> CreateAsync()
        {
            return await _faceService.CreatePerson(User.Picture, User.Name);
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
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "RateLimitExceeded")
                {
                    Debug.WriteLine($"GetUserAsync: RateLimitExceeded: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<ServiceResponse> IdentifyFaces(SoftwareBitmap bitmap)
        {
            _lastFrameShot = await _fileService.EncodedBytes(bitmap);

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
            _toothbrushingLauncher.Tick -= ToothbrushingLauncher;
            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
            OnReset();
            base.Cleanup();
        }
    }
}
