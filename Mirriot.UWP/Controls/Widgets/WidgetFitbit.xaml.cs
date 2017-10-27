using Miriot.Win10.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Miriot.Common.Model;
using Miriot.Services;
using Miriot.Core.ViewModels.Widgets;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetFitbit : IWidgetBase
    {
        public OAuth2AccessToken AccessToken;

        public WidgetFitbit(FitbitModel widget) : base(widget)
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            try
            {
                OAuth2Helper authenticator = new OAuth2Helper("227H9T", "74e4a047fce979dd36a0edbd626c3939", "http://Miriot.Win10.suismoi.fr");
                string[] scopes = new[] { "profile", "weight" };

                string authUrl = authenticator.GenerateAuthUrl(scopes, null);

                WebView browser = new WebView();
                browser.Height = 300;
                browser.Width = 300;
                browser.Source = new Uri(authUrl);
                browser.DefaultBackgroundColor = Colors.Black;
                MainGrid.Children.Add(browser);

                browser.NavigationStarting += async (s, args) =>
                {
                    if (args.Uri.OriginalString.Contains("code="))
                    {
                        var code = args.Uri.Query.Replace("?code=", "");
                        AccessToken = await authenticator.ExchangeAuthCodeForAccessTokenAsync(code);
                        AccessToken = await authenticator.RefreshToken(AccessToken.RefreshToken);

                        MainGrid.Children.Remove(browser);

                        try
                        {
                            var w = await GetWeightAsync(DateTime.Now, null);

                            Value.Text = $"{w.Weight.First().Weight}kg";
                        }
                        catch (Exception ex)
                        {
                            // Access denied ?
                            Debug.WriteLine(ex.Message);
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                // Something went wrong
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<Weights> GetWeightAsync(DateTime startDate, DateTime? endDate)
        {
            string apiCall;
            if (endDate == null)
            {
                apiCall = string.Format("/1/user/{0}/body/log/weight/date/{1}/{2}.json", AccessToken.UserId, startDate.ToString("yyyy-MM-dd"), "1m");
            }
            else
            {
                if (startDate.AddDays(31) < endDate)
                {
                    throw new ArgumentOutOfRangeException("31 days is the max span. Try using period format instead for longer: https://wiki.fitbit.com/display/API/API-Get-Body-Weight");
                }

                apiCall = string.Format("/1/user/{0}/body/log/weight/date/{1}/{2}.json", AccessToken.UserId, startDate.ToString("yyyy-MM-dd"), endDate.Value.ToString("yyyy-MM-dd"));
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.fitbit.com");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", AccessToken.Token);
                client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

                var response = await client.GetAsync(apiCall);
                var r = response.IsSuccessStatusCode ? (await response.Content.ReadAsStringAsync()) : null;

                var w = JsonConvert.DeserializeObject<Weights>(r);
                return w;
            }
        }

        public void SetPosition(int x, int y)
        {
            Grid.SetColumn(this, x);
            Grid.SetRow(this, y);
        }
    }

    public class Weights
    {
        public List<WeightLog> Weight { get; set; }
    }

    public class WeightLog
    {
        public long LogId { get; set; }
        public float Bmi { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public float Weight { get; set; }
        public DateTime DateTime { get { return Date.Date.Add(Time.TimeOfDay); } }
    }
}
