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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetNews
    {
        public List<Article> Articles { get; set; }

        public WidgetNews(NewsModel widget) : base(widget)
        {
            InitializeComponent();
            Get();
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

                    Articles = new List<Article>(news.articles);

                    Rotator.ItemsSource = Articles;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NewsWidget error: {ex.Message}");
            }
        }

        public override void SetPosition(int? x, int? y)
        {
            base.SetPosition(x, y);
            Grid.SetRowSpan(this, 2);
        }
    }
}
