using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using Microsoft.ProjectOxford.Common;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Controls;
using Miriot.Core.Messages;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels;
using Miriot.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
    public sealed partial class MainPage
    {
        private bool _isProcessing;
        private CoreDispatcher _dispatcher;
        private ColorBloomTransitionHelper _transition;
        private readonly IFrameAnalyzer<ServiceResponse> _frameAnalyzer;
        private int _noFaceDetectedCount;

        private MainViewModel Vm => DataContext as MainViewModel;

        #region Ctor
        public MainPage()
        {
            InitializeComponent();
            InitializeTransitionHelper();

            Loaded += MainPage_Loaded;
            _frameAnalyzer = ServiceLocator.Current.GetInstance<IFrameAnalyzer<ServiceResponse>>();
        }

        #endregion

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Bloomer();

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            Task.Run(async () => await _frameAnalyzer.AttachAsync(Camera));
            _frameAnalyzer.AnalysisFunction = Vm.IdentifyFaces;
            _frameAnalyzer.UsersIdentified += OnUsersIdentified;
            _frameAnalyzer.NoFaceDetected += OnNoFaceDetected;
            _frameAnalyzer.OnPreAnalysis += OnStartingIdentification;

            Messenger.Default.Register<ActionMessage>(this, OnAction);

            Vm.PropertyChanged += VmOnPropertyChanged;
        }

        private async void OnAction(ActionMessage msg)
        {
            await DoAction(msg.Intent);
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Vm.Widgets))
            {
                if (Vm.Widgets != null)
                {
                    Vm.Widgets.CollectionChanged -= WidgetsChanged;
                    Vm.Widgets.CollectionChanged += WidgetsChanged;
                }
            }

            if (e.PropertyName == nameof(Vm.SpeakStream))
            {
                if (Vm.SpeakStream != null)
                {
                    MediaElementCtrl.SetSource(Vm.SpeakStream, ((SpeechSynthesisStream) Vm.SpeakStream).ContentType);
                    MediaElementCtrl.Play();
                }
            }
        }

        private void WidgetsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                WidgetZone.Children.Clear();
            }
            else if (e.NewItems.Count > 0)
            {
                LoadWidget((Widget)e.NewItems[0]);
            }
        }

        private async void OnStartingIdentification(object sender, EventArgs eventArgs)
        {
            await RunOnUiThread(() =>
            {
                if (Vm.CurrentState == States.Active)
                {
                    return;
                }

                Vm.IsLoading = true;
                Vm.StateChangedCommand.Execute(States.Active);
            });
        }

        private async void OnNoFaceDetected(object sender, EventArgs e)
        {
            await RunOnUiThread(() =>
            {
                _noFaceDetectedCount++;

                if (_noFaceDetectedCount < 15) return;

                Vm.StateChangedCommand.Execute(States.Inactive);
                CleanUi();
            });
        }

        private async void OnUsersIdentified(object sender, ServiceResponse response)
        {
            await RunOnUiThread(() =>
            {
                _noFaceDetectedCount = 0;

                if (Vm.User != null)
                {
                    if (Vm.IsToothbrushing)
                    {
                        var user = response.Users.FirstOrDefault();
                        if (user != null)
                            SetToothZone(user.FaceRectangle);
                    }
                    return;
                }

                CleanUi();
                Vm.UsersIdentifiedCommand.Execute(response);
            });
        }

        private void SetToothZone(Rectangle userFaceRectangle)
        {
            Canvas.SetTop(ToothIndicator, userFaceRectangle.Top);
            Canvas.SetLeft(ToothIndicator, userFaceRectangle.Left);
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
            var initialBounds = new Windows.Foundation.Rect  // maps to a rectangle the size of the header
            {
                Width = 110,
                Height = 110,
                X = 0,
                Y = 0
            };

            var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window

            _transition.Start(Colors.Black, initialBounds, finalBounds);
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
                    TakePhoto();
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
                    //Vm.SpeakCommand.Execute("Si je m'en tiens aux personnes que je connais, je peux affirmer que tu es le plus beau.");
                    break;
                case "Tweet":

                    break;
                case "HideAll":
                    break;
                case "StartScreen":
                    break;
                case "DisplayWidget":
                    break;
                case "DisplayMail":
                    break;
                case "AddReminder":
                    break;
                case "None":
                    if (Vm.IsListeningFirstName)
                        Vm.Repeat();
                    break;
            }
        }

        private void SetTvScreenSize(bool isFullScreen)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetTv);
            if (w != null)
            {
                ((IWidgetExclusive)w).IsFullscreen = isFullScreen;
            }
        }

        private void TurnOnRadio(IntentResponse intent)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetRadio);

            if (w == null)
            {
                w = new WidgetRadio(null);
                WidgetZone.Children.Add(w);
            }

            ((WidgetRadio)w).DoAction(intent);
        }

        private void TurnOnTv(IntentResponse intent)
        {
            var w = WidgetZone.Children.FirstOrDefault(e => e is WidgetTv);

            if (w == null)
            {
                w = new WidgetTv(null, Vm.User.UserData.CachedTvUrls);
                WidgetZone.Children.Add(w);
            }

            ((WidgetTv)w).TurnOn(intent);
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
                w = new WidgetDeezer(null);
                WidgetZone.Children.Add(w);
            }
            else
            {
                StopSong();
            }

            await ((WidgetDeezer)w).FindTrackAsync(search);
        }

        private async Task RunOnUiThread(DispatchedHandler action)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        private void LoadWidget(Widget widget)
        {
            WidgetBase w;

            switch (widget.Type)
            {
                case WidgetType.Time:
                    w = new WidgetTime(widget);
                    break;
                case WidgetType.Fitbit:
                    w = new WidgetFitbit(widget);
                    break;
                case WidgetType.Calendar:
                    w = new WidgetCalendar(widget);
                    break;
                case WidgetType.Sncf:
                    w = new WidgetSncf(widget);
                    break;
                case WidgetType.Weather:
                    w = new WidgetWeather(widget);
                    break;
                case WidgetType.Horoscope:
                    w = new WidgetHoroscope(widget);
                    break;
                case WidgetType.Sport:
                    w = new WidgetSport(widget);
                    break;
                case WidgetType.Twitter:
                    w = new WidgetTwitter(widget);
                    break;
                default:
                    return;
            }

            if (w is IWidgetListener)
                ((IWidgetListener)w).OnInfosChanged += WidgetInfosChanged;

            // Add widget to grid
            WidgetZone.Children.Add(w);
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

            if (((IWidgetOAuth)w).Token != null)
            {
                w.OriginalWidget.Infos.Add(JsonConvert.SerializeObject(new OAuthWidgetInfo { Token = ((IWidgetOAuth)w).Token }));
            }

            await Vm.UpdateUserAsync();
        }

        private void CleanUi()
        {
            Vm.ResetCommand.Execute(null);
            // Force delete transition
            WidgetZone.Children.Clear();
            InfoUnknownPanel.Opacity = 0;
            Img.Source = null;
            MediaElementCtrl.Stop();
        }

        private void TakePhoto()
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Vm.Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _frameAnalyzer.Cleanup();
            Camera.Cleanup();
            Vm.Cleanup();
            Messenger.Default.Unregister(this);

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
