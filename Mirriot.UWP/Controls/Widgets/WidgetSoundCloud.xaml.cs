using Miriot.JavascriptHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Controls
{
    public sealed partial class WidgetSoundCloud : WidgetBase
    {
        private Random _rnd = new Random();

        public WidgetSoundCloud()
        {
            InitializeComponent();
            IsExclusive = true;
            Load();
        }

        private void Load()
        {
            try
            {
                Browser.NavigationStarting += (a, b) =>
                {
                    // WebView native object must be inserted in the OnNavigationStarting event handler
                    Handler winRTObject = new Handler();
                    winRTObject.OnProgressChanged += (c, d) =>
                    {
                        ProgressCtrl.Value = (c as Handler).Ratio;
                    };

                    // Expose the native WinRT object on the page's global object
                    Browser.AddWebAllowedObject("NotifyApp", winRTObject);
                };

                Browser.Source = new Uri("http://miriot.suismoi.fr/index.html?=" + DateTime.Now.TimeOfDay);
                Browser.DefaultBackgroundColor = Colors.Black;
            }
            catch (Exception ex)
            {
                // Something went wrong
                Debug.WriteLine("Widget SoundCloud:" + ex.Message);
            }
        }

        public async Task StopAsync()
        {
            await Browser.InvokeScriptAsync("Stop", null);
        }

        public async Task FindTrackAsync(string search, string genre)
        {
            var q = string.Empty;
            var genreParam = string.Empty;

            if (!string.IsNullOrEmpty(genre))
                genreParam = $"&genres=%27{genre}%27";

            if (!string.IsNullOrEmpty(search))
                q = $"&q=%27{search}%27";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.soundcloud.com/");
                var res = await client.GetAsync($"tracks?client_id=fd64cdc0a99a9a8bc50269df7e02af71{q}{genreParam}");
                var content = await res.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<SoundCloudResponse>>(content);

                if (data != null && data.Any())
                {
                    var music = data.OrderByDescending(e => e.favoritings_count).First();

                    // If no params, play random music 
                    if (string.IsNullOrEmpty(q))
                    {
                        var i = _rnd.Next(data.Count - 1);
                        music = data[i];
                    }

                    await Play(music);
                }
            }
        }

        public async Task Play(SoundCloudResponse track)
        {
            var path = track.artwork_url?.Replace("-large", "-t300x300");

            if (path == null)
                path = "ms-appx:///Assets/nodisc.png";

            try
            {
                var res = await Browser.InvokeScriptAsync("eval", new[] { "Test('" + track.uri + "').toString()" });
                //await Browser.InvokeScriptAsync("eval", new[] { "SC.Widget(document.getElementById(\"sc-widget\")).load(http://api.soundcloud.com/tracks/13692671, {show_artwork: false, auto_play: true});" });
                //await Browser.InvokeScriptAsync("eval", new string[] { "w = document.getElementById('sc-widget'); ww = SC.Widget(w); ww.load(http://api.soundcloud.com/tracks/13692671, {show_artwork: false, auto_play: true});" });
                //await Browser.InvokeScriptAsync("PlayTrack", new[] { "'" + track.uri + "'" });
                //await Browser.InvokeScriptAsync("eval", new[] { "PlayTrack(" + track.uri + ");" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InvokeScript 'PlayTrack' failed : {ex.Message}");
            }

            Title.Text = track.title;
            ArtworkImg.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }
    }

    public class SoundCloudResponse
    {
        public string title { get; set; }
        public string uri { get; set; }
        public string artwork_url { get; set; }

        public int favoritings_count { get; set; }
    }
}
