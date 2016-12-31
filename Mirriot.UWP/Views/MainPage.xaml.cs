using Microsoft.Practices.ServiceLocation;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Controls;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels;
using Miriot.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Window = Windows.UI.Xaml.Window;

namespace Miriot
{
    public sealed partial class MainPage : Page
    {
        private bool _isProcessing;
        private string _currentPicturePath;
        private CoreDispatcher _dispatcher;
        private DispatcherTimer _timer;
        private DispatcherTimer _sensorTimer;
        private SpeechRecognizer _speechRecognizer;
        private SpeechSynthesizer _speechSynthesizer;
        private bool _isListeningFirstName;
        private bool _isListeningYesNo;
        private ColorBloomTransitionHelper _transition;

        //private readonly FrameGrabber<LiveCameraResult> _grabber = null;
        private readonly FrameAnalyzer _frameAnalyzer = new FrameAnalyzer();


        public MainViewModel Vm => ServiceLocator.Current.GetInstance<MainViewModel>();

        #region Ctor
        public MainPage()
        {
            InitializeComponent();
            InitializeTransitionHelper();

            //if (Vm.IsMobile)
                Camera.ShowPreview = true;

            Loaded += MainPage_Loaded;
        }

        #endregion

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Bloomer();

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            //UpdateVisualState(States.Inactive);

            Task.Run(async () => await _frameAnalyzer.AttachAsync(Camera));
            
            //Task.Run(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await InitSensorAsync().ConfigureAwait(true)));

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 30);
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// All of the Color Bloom transition functionality is encapsulated in this handy helper
        /// which we will init once
        /// </summary>
        private void InitializeTransitionHelper()
        {
            // we pass in the UIElement that will host our Visuals
            _transition = new ColorBloomTransitionHelper(HostForVisual);

            // when the transition completes, we need to know so we can update other property values
            _transition.ColorBloomTransitionCompleted += ColorBloomTransitionCompleted;
        }

        private void Bloomer()
        {
            var initialBounds = new Windows.Foundation.Rect()  // maps to a rectangle the size of the header
            {
                Width = 110,
                Height = 110,
                X = 0,
                Y = 0
            };

            var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window

            _transition.Start(Colors.Black, initialBounds, finalBounds);

        }

    
        /// <summary>
        /// Initialize ultrasonic sensor
        /// </summary>
        /// <returns>nothing</returns>
        private async Task InitSensorAsync()
        {
            // Instanciate sensor trig is pin 23 & echo is pin 24
            var ultrasonicDistanceSensor = new UltrasonicDistanceSensor(23, 24);

            try
            {
                // Initialize sensor
                await ultrasonicDistanceSensor.InitAsync();

                // Timer 400ms
                _sensorTimer = new DispatcherTimer();
                _sensorTimer.Interval = TimeSpan.FromMilliseconds(1000);
                _sensorTimer.Tick += async (a, b) =>
                {
                    // Retrieve distance in cm
                    var d = await ultrasonicDistanceSensor.GetDistanceAsync();

                    // Log
                    WriteData(Math.Round(d));

                    // Use to force process
                    if (d < 30)
                    {
                        if (Vm.CurrentState == States.Active)
                        {
                            CleanUI();
                            _isProcessing = false;
                            UpdateVisualState(States.Inactive);
                        }
                        else
                        {
                            // Ignore distance if processing
                            if (_isProcessing) return;

                            _isProcessing = true;
                            await ProceedAsync();
                            _isProcessing = false;
                        }
                    }
                };

                _sensorTimer.Start();
            }
            catch (Exception)
            {
                Vm.HasSensor = false;
                UpdateVisualState(States.Active);
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();

            // Don't clean UI for Mobile apps
            if (Vm.IsMobile || !Vm.HasSensor)
                return;

            if (Vm.CurrentState == States.Active)
                UpdateVisualState(States.Passive);

            CleanUI();
        }

        private void WriteData(double distance)
        {
            Debug.WriteLine($"{distance} cm");
        }

        private void UpdateVisualState(States state)
        {
            // If state is already the current one
            if (Vm.CurrentState == state) return;

            // Set current state
            Vm.CurrentState = state;
        }

        /// <summary>
        /// Proceder au traitement
        /// </summary>
        /// <returns>nothing</returns>
        private async Task ProceedAsync()
        {
            // Some UI things
            UpdateVisualState(States.Active);
            CleanUI();
            Vm.IsLoading = true;

            Debug.WriteLine("Take photo started");

            // Take a photo and get its path
            var uri = await Camera.TakePhotoAsync();

            Debug.WriteLine("Take photo ended");

            //MOCKED
            //var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            //uri = p.Path + "/untitled.png";

            // Post photo to Azure 
            // Compare faces & return identified user
            ServiceResponse response = await Vm.GetUserAsync(uri);

            Vm.User = response?.User;

            // Use to create the profil directly from Speech
            if (!string.IsNullOrEmpty(uri))
                _currentPicturePath = uri;

            // User has been identified
            if (Vm.User != null)
            {
                if (Vm.User.UserData.Widgets != null)
                {
                    await LoadWidgetsAsync(Vm.User.UserData.Widgets);

                    //SubtitleTxt.Text = !string.IsNullOrEmpty(_user.FriendlyEmotion) ? $"Vous avez l'air {_user.FriendlyEmotion} aujourd'hui" : string.Empty;
                }
                else
                {
                    // In case of the user has no widgets
                    Vm.User.UserData.Widgets = new List<Widget> { new Widget { Type = WidgetType.Time } };
                    await LoadWidgetsAsync(Vm.User.UserData.Widgets);
                    //SubtitleTxt.Text = !string.IsNullOrEmpty(_user.FriendlyEmotion) ? $"Vous avez l'air {_user.FriendlyEmotion} aujourd'hui" : string.Empty;
                }

                StartListening();

                Vm.User.Emotion = await Vm.GetEmotionAsync(_currentPicturePath, Vm.User.FaceRectangleTop, Vm.User.FaceRectangleLeft);
            }
            else
            {
                await ContinueProcess(response);
            }

            Vm.IsLoading = false;

        }

        private async Task ContinueProcess(ServiceResponse response)
        {
            if (response != null && response.Error.HasValue)
            {
                if (response.Error.Value == ErrorType.NoFaceDetected)
                {
                    WelcomeTxt.Text = "Je n'y vois pas très clair...";
                    SubtitleTxt.Text = "Aucun visage n'a été identifé.";
                }
                else if (response.Error.Value == ErrorType.UnknownFace)
                {
                    await PromptForUnknownFace();
                }
                else
                {
                    SetWelcomeMessage(null);
                }
            }
        }

        private async Task PromptForUnknownFace()
        {
            await SetMessage("Bonjour. Je m'appelle MirioT.", "Quel est votre prénom ? (dites: je m'appelle...)");

            await Speak("Bonjour. Je m'appelle Miriotte. Et vous ? Quel est votre prénom ?");

            _isListeningFirstName = true;

            StartListening();
        }

        private async Task RepeatPromptForUnknownFace()
        {
            await SetMessage("Je n'ai pas compris", "Quel est votre prénom ? (dites: je m'appelle...)");

            await Speak("Je n'ai pas compris. Quel est votre prénom ?");

            _isListeningFirstName = true;

            StartListening();
        }

        #region Speech
        private async void InitializeSpeech()
        {
            try
            {
                _speechRecognizer = new SpeechRecognizer(new Windows.Globalization.Language("fr-FR"));
            }
            catch (Exception)
            {
                Debug.WriteLine("SpeechRecognizer failed to initialize : check the microphone");
                return;
            }

            await _speechRecognizer.CompileConstraintsAsync();

            // Stop listening events
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;

            // Start listening events
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            _speechSynthesizer = new SpeechSynthesizer
            {
                Voice = (from voiceInformation in SpeechSynthesizer.AllVoices
                    select voiceInformation).First(e => e.Language == "fr-FR")
            };
        }

        #region Continuous Recognition

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var text = CleanForDemo(args.Result.Text);

            await SetMessage("Un instant...", "je réfléchis...");

            await RunOnUiThread(() => Vm.IsLoading = true);

            if (_isListeningYesNo)
            {
                _isListeningYesNo = false;

                if (text.Contains("oui"))
                {
                    _isListeningFirstName = false;

                    await CreateProfile();
                }
                else if (text.Contains("non"))
                {
                    await RepeatPromptForUnknownFace();
                }
                else
                {
                    await SetMessage(string.Empty, "Répondez par oui ou par non");
                }
            }
            else
            {
                // Appel au service LUIS
                var res = await AskLuisAsync(text);

                // Récupération de la première action (intent avec Score le plus élevé)
                var intent = res?.Intents?.OrderByDescending(e => e.Score).FirstOrDefault();

                if (_isListeningFirstName)
                {
                    if (intent != null && intent.Intent == "CreateProfile")
                        await SetNewProfileMessage(intent);
                    else
                        await RepeatPromptForUnknownFace();
                }
                else
                {
                    if (intent != null)
                    {
                        await RunOnUiThread(async () => await DoAction(intent));
                    }

                    await SetMessage(string.Empty, string.Empty);
                }
            }

            await RunOnUiThread(() => { Vm.IsLoading = false; });
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
            var isSuccess = await Vm.CreateAsync(Vm.User.Name, _currentPicturePath);

            if (isSuccess)
            {
                await SetMessage($"Très bien. Ravi de faire votre connaissance {Vm.User.Name}.", "Utilisez votre téléphone pour ajouter des widgets");
                await Speak($"Très bien. Ravi de faire votre connaissance {Vm.User.Name}.");
            }
            else
            {
                await SetMessage(null, "Impossible d'enregistrer le compte");
            }
        }

        private async Task DoAction(IntentResponse intent)
        {
            switch (intent.Intent)
            {
                case "PlaySong":
                    await TurnOff();
                    await PlaySong(intent);
                    break;
                case "TakePhoto":
                    Shoot_Click(null, null);
                    break;
                case "TurnOnTv":
                    await TurnOff();
                    TurnOnTv(intent);
                    break;
                case "TurnOff":
                    await TurnOff();
                    break;
                case "FullScreenTv":
                    SetTvScreenSize(true);
                    break;
                case "ReduceScreenTv":
                    SetTvScreenSize(false);
                    break;
                case "TurnOnRadio":
                    await TurnOff();
                    TurnOnRadio(intent);
                    break;
                case "BlancheNeige":
                    Speak("Si je m'en tiens aux personnes que je connais, je peux affirmer que tu es le plus beau.");
                    //Speak("Non mais tu t'es cru où là ? Dans Blanche neige ou quoi ?");
                    break;
                case "None":
                    if (_isListeningFirstName)
                        await Repeat();
                    break;
            }
        }

        private async Task Repeat()
        {
            await RepeatPromptForUnknownFace();
        }

        private async Task SetNewProfileMessage(IntentResponse intent)
        {
            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string name = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null && p.Name == "Firstname")
                        name = p.Value.OrderByDescending(e => e.Score).First().Entity;
                }

            Vm.User = new User { Name = name };

            await SetMessage($"Bonjour {name.ToUpperInvariant()}", "Aie-je bien entendu votre prénom ?");
            await Speak($"Bonjour {name.ToUpperInvariant()}. Aie-je bien entendu votre prénom ?");

            _isListeningYesNo = true;
        }

        private void SetTvScreenSize(bool isFullScreen)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetTv);
            if (w != null)
            {
                ((WidgetBase)w).IsFullscreen = isFullScreen;

                if (isFullScreen)
                {
                    Grid.SetColumnSpan((WidgetTv)w, 4);
                    Grid.SetRowSpan((WidgetTv)w, 4);
                }
                else
                {
                    Grid.SetColumnSpan((WidgetTv)w, 1);
                    Grid.SetRowSpan((WidgetTv)w, 1);
                }
            }
        }

        private void TurnOnRadio(IntentResponse intent)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetRadio);
            if (w != null)
            {
                ((WidgetRadio)w).TurnOn(intent);
            }
            else
                WidgetZone.Children.Add(new WidgetRadio(intent));
        }

        private void TurnOnTv(IntentResponse intent)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetTv);
            if (w != null)
            {
                ((WidgetTv)w).TurnOn(intent);
            }
            else
                WidgetZone.Children.Add(new WidgetTv(intent, Vm.User.UserData.CachedTvUrls));
        }

        private async Task TurnOff()
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetTv);
            if (w != null)
            {
                WidgetZone.Children.Remove(w);
            }

            w = WidgetZone.Children.FirstOrDefault(e => e is WidgetRadio);
            if (w != null)
            {
                WidgetZone.Children.Remove(w);
            }

            w = WidgetZone.Children.FirstOrDefault(e => e is WidgetDeezer);
            if (w != null)
            {
                await StopSong();
                WidgetZone.Children.Remove(w);
            }
        }

        private async Task StopSong()
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetDeezer);
            if (w == null) return;

            await ((WidgetDeezer)w).StopAsync();
        }

        private async Task PlaySong(IntentResponse intent)
        {
            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string search = string.Empty;
            string genre = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null)
                    {
                        if (p.Name == "Search")
                            search = p.Value.OrderByDescending(e => e.Score).First().Entity;
                        if (p.Name == "Genre")
                            genre = p.Value.OrderByDescending(e => e.Score).First().Entity;
                    }
                }

            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetDeezer);
            if (w == null)
            {
                w = new WidgetDeezer();
                WidgetZone.Children.Add(w);
            }
            else
            {
                StopSong();
            }

            await ((WidgetDeezer)w).FindTrackAsync(search);
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

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    // Enable continuous listening
                    StartListening();
                }
            }
        }
        #endregion

        private async void StartListening()
        {
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                try
                {
                    await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception ex)
                {
                    InitializeSpeech();
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async void Stop()
        {
            if (_speechRecognizer?.State != SpeechRecognizerState.Idle)
            {
                try
                {
                    var asyncAction = _speechRecognizer?.ContinuousRecognitionSession?.StopAsync();
                    if (asyncAction != null)
                        await asyncAction;
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        /// <summary>
        /// Make the voice speak the text
        /// When the voice has ended, start listening
        /// </summary>
        /// <param name="text">Text to be spoken</param>
        /// <returns>nothing</returns>
        private async Task Speak(string text)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
                MediaElementCtrl.SetSource(stream, stream.ContentType);
                MediaElementCtrl.Play();
            });
        }

        #endregion

        private async Task RunOnUiThread(DispatchedHandler action)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        private void SetWelcomeMessage(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                WelcomeTxt.Text = string.Empty;
                SubtitleTxt.Text = "Vérifiez votre connexion internet";
                return;
            }

            WelcomeTxt.Text = $"Bonjour {Vm.User.Name}";
        }

        private async Task SetMessage(string title, string subTitle)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                WelcomeTxt.Text = title;
                SubtitleTxt.Text = subTitle;
            });
        }

        private async Task LoadWidgetsAsync(List<Widget> widgets)
        {
            Vm.Widgets = new ObservableCollection<IWidgetBase>();

            foreach (var widget in widgets)
            {
                WidgetBase w;

                switch (widget.Type)
                {
                    case WidgetType.Time:
                        w = new WidgetTime();
                        break;
                    case WidgetType.Fitbit:
                        w = new WidgetFitbit();
                        break;
                    case WidgetType.Calendar:
                        w = new WidgetCalendar(widget);
                        break;
                    case WidgetType.Sncf:
                        w = new WidgetSncf();
                        break;
                    case WidgetType.Weather:
                        w = new WidgetWeather(widget);
                        break;
                    case WidgetType.Sport:
                        var sport = JsonConvert.DeserializeObject<SportWidgetInfo>(widget.Infos.First());
                        w = new WidgetSport(sport);
                        break;
                    default:
                        continue;
                }

                w.OriginalWidget = widget;
                w.OnInfosChanged += WidgetInfosChanged;

                // Set Widget's margin
                w.Margin = new Thickness(20);

                if (Vm.IsMobile)
                {
                    // Set Widget's position in grid
                    w.SetAlignment(widget.X, widget.Y);

                    // Add widget to grid
                    WidgetMobileZone.Children.Add(w);
                }
                else
                {
                    // Set Widget position in grid
                    Grid.SetColumn(w, widget.X);
                    Grid.SetRow(w, widget.Y);

                    if (w is WidgetCalendar)
                        Grid.SetRowSpan(w, 2);

                    // Add widget to grid
                    WidgetZone.Children.Add(w);
                }

                Vm.Widgets.Add(w);

                // Wait 300ms to create a better transition effect
                await Task.Delay(300);
            }
        }

        private async void WidgetInfosChanged(object sender, EventArgs e)
        {
            var w = sender as WidgetBase;

            if (w.OriginalWidget.Infos == null)
                w.OriginalWidget.Infos = new List<string>();
            else
            {
                var entry = w.OriginalWidget.Infos.FirstOrDefault(s => JsonConvert.DeserializeObject<OAuthWidgetInfo>(s)?.Token != null);

                if (entry != null)
                    w.OriginalWidget.Infos.Remove(entry);
            }

            if (w.Token != null)
            {
                w.OriginalWidget.Infos.Add(JsonConvert.SerializeObject(new OAuthWidgetInfo { Token = w.Token }));
            }

            await Vm.UpdateUserAsync();
        }

        private void CleanUI()
        {
            // Force delete transition
            WidgetMobileZone.Children.Clear();
            WidgetZone.Children.Clear();
            WelcomeTxt.Text = string.Empty;
            SubtitleTxt.Text = string.Empty;

            InfoUnknownPanel.Opacity = 0;
            _timer.Stop();
            _isListeningFirstName = false;
            _isListeningYesNo = false;
            Img.Source = null;

            Stop();
        }

        #region Methods for debug mode
        private void Detect_Click(object sender, RoutedEventArgs e)
        {
            if (!_isProcessing)
            {
                _isProcessing = true;
                Task.Run(async () => await RunOnUiThread(async () => await ProceedAsync()));
                _isProcessing = false;
            }
        }

        private async void Speak_Click(object sender, RoutedEventArgs e)
        {
            await DoAction(new IntentResponse { Intent = "TurnOnTv", Score = 1, Actions = new List<Common.Action> { new Common.Action { Name = "TurnOnTv", Parameters = new List<Parameter>() { new Parameter { Name = "Channel", Value = new List<ParameterValue> { new ParameterValue { Entity = "france deux" } } } }, Triggered = true } } });
        }

        private void Shoot_Click(object sender, RoutedEventArgs e)
        {
            if (!_isProcessing)
            {
                _isProcessing = true;
                Task.Run(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var path = await Camera.TakePhotoAsync();
                    if (path != null)
                        Img.Source = new BitmapImage(new Uri(path));
                }));

                _isProcessing = false;
            }
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitializeSpeech();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Camera.Cleanup();
            Stop();
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Updates the background of the layout panel to the same color whose transition animation just completed.
        /// </summary>
        private void ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            //// Grab an item off the pending transitions queue
            //var item = pendingTransitions.Dequeue();

            //// now remember, that bloom animation was just transitional
            //// so we need to explicitly set the correct color as background of the layout panel
            //var header = (AppBarButton)item.Header;
            MainGrid.Background = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// In response to a XAML layout event on the Grid (named UICanvas) we will apply a clip
        /// to ensure all Visual animations stay within the bounds of the Grid, and doesn't bleed into
        /// the top level Frame belonging to the Sample Gallery. Probably not a factor in most other cases.
        /// </summary>
        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uiCanvasLocation = MainGrid.TransformToVisual(MainGrid).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(uiCanvasLocation, e.NewSize)
            };
            MainGrid.Clip = clip;
        }
    }
}
