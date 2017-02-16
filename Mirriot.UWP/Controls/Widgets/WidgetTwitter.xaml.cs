using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using System;
using System.Diagnostics;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Miriot.Common;

namespace Miriot.Controls
{
    public sealed partial class WidgetTwitter : IWidgetOAuth, IWidgetExclusive, IWidgetAction
    {
        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }

        public string Token { get; set; }

        public WidgetTwitter(Widget widget)
        {
            OriginalWidget = widget;

            InitializeComponent();

            Margin = new Thickness(0);

            Loaded += WidgetTwitter_Loaded;
        }

        private async void WidgetTwitter_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var twitter = ServiceLocator.Current.GetInstance<ITwitterService>();

                // Get current user info
                var user = await twitter.GetUserAsync();

                // Get user timeline
                ListView.ItemsSource = await twitter.GetHomeTimelineAsync();
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
}
