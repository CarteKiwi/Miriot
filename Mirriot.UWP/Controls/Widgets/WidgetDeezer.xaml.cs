using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using Miriot.Common;

namespace Miriot.Controls
{
    public sealed partial class WidgetDeezer : IWidgetAction, IWidgetExclusive
    {
        private bool _isPlaying;
        private readonly Random _rnd = new Random();

        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }

        public WidgetDeezer(Widget widget) : base(widget)
        {
            InitializeComponent();
            IsExclusive = true;
        }

        public async Task StopAsync()
        {
            _isPlaying = false;
            //await Browser.InvokeScriptAsync("Stop", null);
        }

        public async void DoAction(IntentResponse intent)
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

            await FindTrackAsync(search);
        }

        public async Task FindTrackAsync(string search)
        {
            var q = string.Empty;

            if (!string.IsNullOrEmpty(search))
                q = $"%27{search}%27";
            //else
            //    q = "%27all%20in%20you%20synapson%27";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.deezer.com/");
                var res = await client.GetAsync($"search?q={q}");
                var content = await res.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<DeezerResponse>(content);

                var data = response.data;
                if (data != null && data.Any())
                {
                    var music = data.First();

                    // If no params, play random music 
                    if (string.IsNullOrEmpty(q))
                    {
                        var i = _rnd.Next(data.Count() - 1);
                        music = data[i];
                    }

                    await Play(music);
                }
            }
        }

        public async Task Play(DeezerTrack track)
        {
            if (_isPlaying)
                await StopAsync();

            _isPlaying = true;
            var path = track?.album?.cover_medium;

            if (path == null)
                path = "ms-appx:///Assets/nodisc.png";

            try
            {
                Player.Source = new Uri(track.preview, UriKind.Absolute);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Deezer: Player failed : {ex.Message}");
            }

            Title.Text = track.title;
            ArtworkImg.ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }
    }

    public class DeezerResponse
    {
        public DeezerTrack[] data { get; set; }
    }

    public class DeezerTrack
    {
        public string id { get; set; }
        public bool readable { get; set; }
        public string title { get; set; }
        public string title_short { get; set; }
        public string title_version { get; set; }
        public string link { get; set; }
        public string duration { get; set; }
        public string rank { get; set; }
        public bool explicit_lyrics { get; set; }
        public string preview { get; set; }
        public Artist artist { get; set; }
        public Album album { get; set; }
        public string type { get; set; }
    }

    public class Artist
    {
        public string id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string picture { get; set; }
        public string picture_small { get; set; }
        public string picture_medium { get; set; }
        public string picture_big { get; set; }
        public string tracklist { get; set; }
        public string type { get; set; }
    }

    public class Album
    {
        public string id { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public string cover_small { get; set; }
        public string cover_medium { get; set; }
        public string cover_big { get; set; }
        public string tracklist { get; set; }
        public string type { get; set; }
    }
}
