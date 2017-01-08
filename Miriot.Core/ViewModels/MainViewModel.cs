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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
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
        private readonly INavigationService _navigationService;
        private readonly IFaceService _faceService;
        private readonly IVisionService _visionService;
        private User _user;
        private ObservableCollection<Widget> _widgets;
        private bool _isInternetAvailable;
        private States _currentState;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        #endregion

        #region Commands
        public RelayCommand<ServiceResponse> UsersIdentifiedCommand { get; private set; }
        public RelayCommand<string> ProceedSpeechCommand { get; private set; }
        public RelayCommand<States> StateChangedCommand { get; private set; }
        public RelayCommand<string> ActionNavigateTo { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
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

        private string _title;
        private string _subTitle;
        private bool _isListeningYesNo;
        public bool IsListeningFirstName { get; set; }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

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
            ProceedSpeechCommand = new RelayCommand<string>(OnProceedSpeech);
            UsersIdentifiedCommand = new RelayCommand<ServiceResponse>(OnUsersIdentified);
            StateChangedCommand = new RelayCommand<States>(OnStateChanged);
            ActionNavigateTo = new RelayCommand<string>(OnNavigateTo);
            ResetCommand = new RelayCommand(OnReset);
        }

        private void OnReset()
        {

            IsListeningFirstName = false;
            _isListeningYesNo = false;
            User = null;
            Title = null;
            SubTitle = null;
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
            text = CleanForDemo(text);

            SetMessage("Un instant...", "je réfléchis...");

            IsLoading = true;

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

            User = new User { Name = name, Picture = LastFrameShot };

            SetMessage($"Bonjour {name.ToUpperInvariant()}", "Aie-je bien entendu votre prénom ?");
            Speak($"Bonjour {name.ToUpperInvariant()}. Aie-je bien entendu votre prénom ?");

            _isListeningYesNo = true;
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
                    await LoadUsers(user);
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
                await Task.Delay(300, _cancellationToken.Token);
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

        private void PromptForUnknownFace()
        {
            SetMessage("Bonjour. Je m'appelle MirioT.", "Quel est votre prénom ? (dites: je m'appelle...)");
            Speak("Bonjour, je m'appelle Miriotte. Et vous ? Quel est votre prénom ?");

            IsListeningFirstName = true;

            Messenger.Default.Send(new ListeningMessage());
        }

        private void RepeatPromptForUnknownFace()
        {
            SetMessage("Je n'ai pas compris", "Quel est votre prénom ? (dites: je m'appelle...)");
            Speak("Je n'ai pas compris. Quel est votre prénom ?");

            IsListeningFirstName = true;

            Messenger.Default.Send(new ListeningMessage());
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

        private void Speak(string text)
        {
            Messenger.Default.Send(new SpeakMessage(text));
        }

        private void OnStateChanged(States state)
        {
            if (state == States.Inactive)
            {
                _cancellationToken.Cancel();

                Widgets?.Clear();
            }

            CurrentState = state;
            IsLoading = false;
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
