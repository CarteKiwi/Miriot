using Miriot.Common.Model;
using Newtonsoft.Json;
using System.Linq;
using Miriot.Core.ViewModels.Widgets;
using System.Net.Http;
using System;
using Miriot.Model.Widgets.LeMonde;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetNews
    {
        public WidgetNews(NewsModel widget) : base(widget)
        {
            InitializeComponent();

            //var info = JsonConvert.DeserializeObject<SportWidgetInfo>(widget.Infos.First());

            //TitleTb.Text = info.Competition;
            //Score1Tb.Text = info.Score1.ToString();
            //Score2Tb.Text = info.Score2.ToString();
            //Team1Tb.Text = info.Team1;
            //Team2Tb.Text = info.Team2;
            Get();
        }

        private async void Get()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://www.lemonde.fr/rss/une.xml");
                    var res = await client.GetAsync("");
                    var c = await res.Content.ReadAsStringAsync();

                    var news = JsonConvert.DeserializeObject<LeMondeResponse>(c);

                    Picture.Source = new BitmapImage(new Uri("", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NewsWidget error: {ex.Message}");
            }
        }
    }
}
