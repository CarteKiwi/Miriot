using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Miriot.Controls
{
    public sealed partial class WidgetSncf
    {
        private DateTime? _departureDate;
        private DateTime? _nextDepartureDate;
        private bool _isBusy;

        public WidgetSncf(SncfModel widget) : base(widget)
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(1000) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async Task Load()
        {
            _isBusy = true;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.sncf.com/v1/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var req = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "ef136fc4-1d02-46a9-b80f-2e858b775635", "")));
                    AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Basic", req);
                    client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

                    //Gare d'Asnières
                    var gareId = "OCE:SA:87381137";
                    var response = await client.GetAsync("coverage/sncf/stop_areas/stop_area:" + gareId + "/departures?datetime=" + DateTime.Now.ToUniversalTime());
                    var r = response.IsSuccessStatusCode ? (await response.Content.ReadAsStringAsync()) : null;

                    var stopAsnieres = JsonConvert.DeserializeObject<SncfResponse>(r);
                    var trainToParis = stopAsnieres.departures.Where(e => e.display_informations.direction.Contains("Paris")).Take(2);

                    var date1 = trainToParis.ElementAt(0).stop_date_time.arrival_date_time;
                    _departureDate = new DateTime(int.Parse(date1.Substring(0, 4)), int.Parse(date1.Substring(4, 2)), int.Parse(date1.Substring(6, 2)), int.Parse(date1.Substring(9, 2)), int.Parse(date1.Substring(11, 2)), int.Parse(date1.Substring(13, 2)));

                    var date2 = trainToParis.ElementAt(1).stop_date_time.arrival_date_time;
                    _nextDepartureDate = new DateTime(int.Parse(date2.Substring(0, 4)), int.Parse(date2.Substring(4, 2)), int.Parse(date2.Substring(6, 2)), int.Parse(date2.Substring(9, 2)), int.Parse(date2.Substring(11, 2)), int.Parse(date2.Substring(13, 2)));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Widget Sncf:" + ex.Message);
            }

            _isBusy = false;
        }

        private void Timer_Tick(object sender, object e)
        {
            if (_departureDate.HasValue)
            {
                if (_departureDate < DateTime.Now)
                {
                    _departureDate = null;
                    return;
                }

                Departure.Visibility = Visibility.Visible;
                Loader.Visibility = Visibility.Collapsed;

                var date = _departureDate.Value - DateTime.Now;
                DepartureMin.Text = (date.Minutes).ToString();

                if (_nextDepartureDate.HasValue)
                {
                    var date2 = _nextDepartureDate.Value - DateTime.Now;
                    NextDepartureMin.Text = (date2.Minutes).ToString();
                }
            }
            else
            {
                if (!_isBusy)
                {
                    Task.Run(async () => await Load());

                    Loader.Visibility = Visibility.Visible;
                    Departure.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
