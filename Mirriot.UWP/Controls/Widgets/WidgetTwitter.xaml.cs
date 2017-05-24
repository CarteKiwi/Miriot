using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Miriot.Common;
using Miriot.Common.Model.Widgets.Twitter;

namespace Miriot.Controls
{
    public sealed partial class WidgetTwitter : IWidgetOAuth, IWidgetExclusive, IWidgetAction
    {
        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }

        public string Token { get; set; }

        public ObservableCollection<Tweet> Tweets { get; set; }


        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public WidgetTwitter(Widget widget)
        {
            OriginalWidget = widget;

            InitializeComponent();

            Margin = new Thickness(0);

            Loaded += WidgetTwitter_Loaded;
        }

        private void WidgetTwitter_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //var twitter = ServiceLocator.Current.GetInstance<ITwitterService>();

                // Get current user info
                //var user = await twitter.GetUserAsync();

                var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 6) };
                timer.Tick += TweetCheck_Tick;
                timer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void TweetCheck_Tick(object sender, object e)
        {
            await _semaphoreSlim.WaitAsync();

            await LoadTweetsAsync();

            _semaphoreSlim.Release();
        }

        private async Task LoadTweetsAsync()
        {
            try
            {
                var twitter = ServiceLocator.Current.GetInstance<ITwitterService>();

                // Get user timeline
                var tweets = await twitter.GetHomeTimelineAsync();

                if (Tweets == null || !Tweets.Any())
                    Tweets = new ObservableCollection<Tweet>(tweets);
                else
                    foreach (var tweet in tweets.Where(t => !Tweets.Contains(t, new TweetComparer())))
                    {
                        Tweets.Insert(0, tweet);
                    }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async void Tweet(string text)
        {
            var twitter = ServiceLocator.Current.GetInstance<ITwitterService>();
            var statut = await twitter.TweetStatusAsync("Tweet from Miriot");
        }

        public async void Tweet(string text, ImageSource image)
        {
            var twitter = ServiceLocator.Current.GetInstance<ITwitterService>();
            var statut = await twitter.TweetStatusAsync("Tweet from Miriot");
        }

        public void DoAction(IntentResponse intent)
        {

        }

        public override void SetPosition(int x, int y)
        {
            Grid.SetRowSpan(this, 2);
            base.SetPosition(x, y);
        }
    }

    internal class TweetComparer : IEqualityComparer<Tweet>
    {
        public bool Equals(Tweet x, Tweet y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Tweet obj)
        {
            return obj.GetHashCode();
        }
    }
}
