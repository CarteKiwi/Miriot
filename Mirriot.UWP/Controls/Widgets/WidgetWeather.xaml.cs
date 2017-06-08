﻿using Miriot.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Windows.UI.Xaml.Controls;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Controls
{
    public sealed partial class WidgetWeather : IWidgetBase
    {
        private readonly string _key = "84bc189921c14c7a98fdea2a98aa11ba";
        private string _location = "paris";

        public WidgetWeather(Widget widget) : base(widget)
        {
            InitializeComponent();

            RetrieveData(widget);

            GetWeather();
        }

        private void RetrieveData(Widget widget)
        {
            var location = widget.Infos?.FirstOrDefault(e => JsonConvert.DeserializeObject<WeatherWidgetInfo>(e).Location != null);
            if (location != null)
                _location = JsonConvert.DeserializeObject<WeatherWidgetInfo>(location).Location;
        }

        private async void GetWeather()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
                    var res = await client.GetAsync($"weather?appid={_key}&q={_location}&units=metric");
                    var c = await res.Content.ReadAsStringAsync();

                    var weather = JsonConvert.DeserializeObject<WeatherResponse>(c);

                    SetImage(weather);
                    SetLocation(weather);
                    SetTemperature(weather);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WeatherWidget error: {ex.Message}");
            }
        }

        private void SetTemperature(WeatherResponse weather)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentUICulture = ci;

            var temp = weather?.main?.temp_max;
            var tempMin = weather?.main?.temp_min;

            double dc;
            if (double.TryParse(temp, out dc))
                Temperature.Text = $"{Math.Round(dc)}";
            else
                Temperature.Text = $"{temp}";

            double tm;
            if (double.TryParse(tempMin, out tm))
                TemperatureMin.Text = $"{Math.Round(tm)}";
            else
                TemperatureMin.Text = $"{tempMin}";
        }

        private void SetLocation(WeatherResponse weather)
        {
            Location.Text = weather?.name;
        }

        private void SetImage(WeatherResponse weather)
        {
            var icon = weather.weather.First().icon;
            string ico;

            var dic = new Dictionary<string, string>()
                {
                    {"01d", "A"},  {"01n", "B"},
                    {"02d", "C"},  {"02n", "D"},
                    {"03d", "G"},  {"03n", "E"},
                    {"04d", "O"},  {"04n", "J"},
                    {"09d", "S"},  {"09n", "S"},
                    {"10d", "R"},  {"10n", "K"},
                    {"11d", "V"},  {"11n", "V"},
                    {"13d", "W"},  {"13n", "W"},
                    {"50d", "d"},  {"50n", "d"},
                };

            if (dic.ContainsKey(icon))
                ico = dic[icon];
            else
                ico = "A";

            PictoFont.Text = ico;
        }

        public void SetPosition(int x, int y)
        {
            Grid.SetColumn(this, x);
            Grid.SetRow(this, y);
        }
    }
}
