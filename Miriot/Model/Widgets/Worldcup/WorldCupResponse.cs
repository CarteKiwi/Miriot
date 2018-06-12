using System;

namespace Miriot.Model.Widgets.Worldcup
{
    public class WorldCupResponse
    {
        public string name { get; set; }
        public Round[] rounds { get; set; }
    }

    public class Round
    {
        public string name { get; set; }
        public Match[] matches { get; set; }
    }

    public class Match
    {
        public int num { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public Team team1 { get; set; }
        public Team team2 { get; set; }
        public object score1 { get; set; }
        public object score2 { get; set; }
        public object score1i { get; set; }
        public object score2i { get; set; }
        public string group { get; set; }
        public Stadium stadium { get; set; }
        public string city { get; set; }
        public string timezone { get; set; }
        public string Title { get; set; }
        public DateTime FriendlyDate { get; set; }
    }

    public class Team
    {
        public string name { get; set; }
        public string code { get; set; }
        public string FlagUri { get { return "https://api.fifa.com/api/v1/picture/flags-fwc2018-4/" + code; } }
    }

    public class Stadium
    {
        public string key { get; set; }
        public string name { get; set; }
    }
}
