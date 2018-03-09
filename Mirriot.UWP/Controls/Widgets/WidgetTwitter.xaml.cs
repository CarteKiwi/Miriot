using Microsoft.Toolkit.Uwp.Services.Twitter;
using Miriot.Common;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetTwitter : IWidgetOAuth, IWidgetExclusive, IWidgetAction, INotifyPropertyChanged
    {
        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }

        public string Token { get; set; }

        private ObservableCollection<ITwitterResult> _tweets;
        private readonly TwitterModel _widget;

        public ObservableCollection<ITwitterResult> Tweets
        {
            get => _tweets;
            private set => Set(ref _tweets, value);
        }

        public WidgetTwitter(TwitterModel widget) : base(widget)
        {
            InitializeComponent();

            Margin = new Thickness(0);

            Loaded += WidgetTwitter_Loaded;
            this._widget = widget;
        }

        private async void WidgetTwitter_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this._widget.Load();

                TwitterService.Instance.Initialize("n4J84SiGTLXHFh7F5mex5PGLZ", "8ht8N38Sh8hrNYgww3XRYS8X6gIcoywFoJYDcAoBoSfZXaKibt", "https://Miriot.Win10.suismoi.fr");

                await LoadTweetsAsync();
                await GetStream();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public override void OnStateChanged()
        {
            base.OnStateChanged();

            switch (State)
            {
                default:
                case WidgetStates.Compact:
                    SetPosition(1, 2);
                    VisualStateManager.GoToState(this, "MinimalState", true);
                    break;
                case WidgetStates.Large:
                    SetPosition(1, 1);
                    VisualStateManager.GoToState(this, "LargeState", true);
                    break;
            }
        }

        private async Task LoadTweetsAsync()
        {
            try
            {
                // Get user timeline
                var tweets = await TwitterService.Instance.RequestAsync(new TwitterDataConfig
                {
                    QueryType = TwitterQueryType.Home
                }, 50);

                if (Tweets == null || !Tweets.Any())
                    Tweets = new ObservableCollection<ITwitterResult>(tweets);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task GetStream()
        {
            await TwitterService.Instance.StartUserStreamAsync(async tweet =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (tweet != null)
                    {
                        if (tweet is TwitterStreamDeletedEvent)
                        {
                            var toRemove = _tweets.Where(t => t is Tweet)
                                .SingleOrDefault(t => ((Tweet)t).Id == ((TwitterStreamDeletedEvent)tweet).Id);

                            if (toRemove != null)
                            {
                                _tweets.Remove(toRemove);
                            }
                        }
                        else
                        {
                            _tweets.Insert(0, tweet);

                            if (State == WidgetStates.Compact)
                                TweetReceivedSb.Begin();
                        }
                    }
                });
            });
        }

        public async void Tweet(string text)
        {
            var statut = await TwitterService.Instance.TweetStatusAsync("Tweet from Miriot");
        }

        public async void Tweet(string text, ImageSource image)
        {
            var statut = await TwitterService.Instance.TweetStatusAsync("Tweet from Miriot");
        }

        public void DoAction(LuisResponse luis)
        {
            // TODO: Switch state
        }

        public override void SetPosition(int? x, int? y)
        {
            Grid.SetRowSpan(this, 2);
            base.SetPosition(x, y);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
