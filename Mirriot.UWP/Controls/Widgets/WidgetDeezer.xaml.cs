using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Miriot.Common.Model;
using Miriot.Services;
using Miriot.Common;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Common.Model.Widgets.Deezer;
using GalaSoft.MvvmLight.Ioc;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetDeezer : IWidgetAction, IWidgetExclusive
    {
        private bool _isPlaying;
        private readonly Random _rnd = new Random();

        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }

        public WidgetDeezer() : base(null)
        {
            InitializeComponent();
            IsExclusive = true;
        }

        public WidgetDeezer(DeezerModel widget) : base(widget)
        {
            InitializeComponent();
            IsExclusive = true;
        }

        public async Task StopAsync()
        {
            _isPlaying = false;
            //await Browser.InvokeScriptAsync("Stop", null);
        }

        public async void DoAction(LuisResponse luis)
        {
            await FindTrackAsync(luis.Entities.OrderByDescending(e => e.Score).FirstOrDefault().Entity);
        }

        public async Task FindTrackAsync(string search)
        {
            var q = string.Empty;

            if (!string.IsNullOrEmpty(search))
                q = $"%27{search}%27";
            //else
            //    q = "%27all%20in%20you%20synapson%27";

            Debug.WriteLine("Deezer API search for: " + search);

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
            //var ble = SimpleIoc.Default.GetInstance<IBluetoothService>();

            if (_isPlaying)
            {
                await StopAsync();

                //await ble.InitializeAsync();
            }

            //ble.Stop();

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
}
