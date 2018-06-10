using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model.Widgets.LeMonde;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetNews
    {
        public List<Article> Articles { get; set; }
        private int _currentIndex;
        private int _maxIndex;

        public WidgetNews(NewsModel widget) : base(widget)
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            timer.Tick += Timer_Tick;
            timer.Start();

            Get();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (Articles != null)
            {
                _currentIndex++;

                if (_currentIndex >= _maxIndex)
                    _currentIndex = 0;

                var article = Articles[_currentIndex];

                Picture.Source = new BitmapImage(new Uri(article.urlToImage, UriKind.Absolute));
            }
        }

        private async void Get()
        {
            try
            {
                var configService = SimpleIoc.Default.GetInstance<IConfigurationService>();
                var config = await configService.GetKeysByProviderAsync("news");
                var key = config["apiKey"];

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://newsapi.org/v2/top-headlines?sources=le-monde&apiKey={key}");
                    var res = await client.GetAsync("");
                    var c = await res.Content.ReadAsStringAsync();

                    var news = JsonConvert.DeserializeObject<LeMondeResponse>(c);

                    _maxIndex = news.totalResults;

                    Articles = new List<Article>(news.articles);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NewsWidget error: {ex.Message}");
            }
        }
    }
}
