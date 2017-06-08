using Miriot.Common.Model;
using Miriot.Common.Model.Widgets.Horoscope;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Controls
{
    public sealed partial class WidgetHoroscope
    {
        private int? _sign;

        public WidgetHoroscope(Widget widget) : base(widget)
        {
            InitializeComponent();

            RetrieveData(widget);

            Get();
        }

        private void RetrieveData(Widget widget)
        {
            var sign = widget.Infos?.FirstOrDefault(e => JsonConvert.DeserializeObject<HoroscopeWidgetInfo>(e).SignId != null);
            if (sign != null)
                _sign = JsonConvert.DeserializeObject<HoroscopeWidgetInfo>(sign).SignId;
        }

        private async void Get()
        {
            try
            {
                if (_sign == null)
                {
                    Text.Text = "Selectionnez votre signe astrologique dans les paramètres.";
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://kindle.mon-horoscope-du-jour.com/mobile/json/");
                    var res = await client.GetAsync($"hq_time_v2.php?sign_id={_sign}&tz=Europe%2FParis&lang=fr");
                    var c = await res.Content.ReadAsStringAsync();

                    var horo = JsonConvert.DeserializeObject<HoroscopeResponse>(c);

                    Text.Text = horo.content[0].section[0].section_content;
                    Sign.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Horoscope/{(Signs)_sign}.png", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HoroscopeWidget error: {ex.Message}");
            }
        }
    }
}
