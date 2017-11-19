using Miriot.Common.Model.Widgets.Horoscope;
using Miriot.Core.ViewModels.Widgets;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetHoroscope
    {
        private HoroscopeModel _model;

        public WidgetHoroscope(HoroscopeModel model) : base(model.X, model.Y)
        {
            _model = model;

            InitializeComponent();

            _model.Load();

            Get();
        }

        private async void Get()
        {
            try
            {
                if (_model.Sign == null)
                {
                    Text.Text = "Selectionnez votre signe astrologique dans les paramètres.";
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://kindle.mon-horoscope-du-jour.com/mobile/json/");
                    var res = await client.GetAsync($"hq_time_v2.php?sign_id={_model.Sign}&tz=Europe%2FParis&lang=fr");
                    var c = await res.Content.ReadAsStringAsync();

                    var horo = JsonConvert.DeserializeObject<HoroscopeResponse>(c);

                    Text.Text = horo.content[0].section[0].section_content;
                    Sign.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Horoscope/{(Signs)_model.Sign}.png", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HoroscopeWidget error: {ex.Message}");
            }
        }
    }
}
