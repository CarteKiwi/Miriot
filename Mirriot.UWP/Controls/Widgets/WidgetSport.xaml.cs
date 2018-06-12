using Miriot.Common.Model;
using Newtonsoft.Json;
using System.Linq;
using Miriot.Core.ViewModels.Widgets;
using System.Net.Http;
using System;
using Miriot.Model.Widgets.Worldcup;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetSport
    {
        public List<Match> Matches { get; set; }

        public WidgetSport(SportModel widget) : base(widget)
        {
            InitializeComponent();
            Get();
        }

        private async void Get()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"https://raw.githubusercontent.com/openfootball/world-cup.json/master/2018/worldcup.json");
                    var res = await client.GetAsync("");
                    var c = await res.Content.ReadAsStringAsync();

                    var news = JsonConvert.DeserializeObject<WorldCupResponse>(c);

                    Matches = new List<Match>();

                    foreach (var round in news.rounds)
                    {
                        foreach (var match in round.matches)
                        {
                            match.FriendlyDate = DateTime.Parse(match.date);
                            match.Title = match.FriendlyDate.ToLongDateString() + " - " + match.group;
                            Matches.Add(match);
                        }
                    }

                    Rotator.ItemsSource = Matches.Where(m => m.FriendlyDate > DateTime.Now).Take(3);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SportWidget error: {ex.Message}");
            }
        }

        public override void SetPosition(int? x, int? y)
        {
            base.SetPosition(x, y);
            Grid.SetRowSpan(this, 2);
        }
    }
}
