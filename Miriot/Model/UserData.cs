using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class MiriotConfiguration : ObservableObject
    {
        public MiriotConfiguration(string id, string name)
        {
            Id = id;
            Name = name;
            Widgets = new List<Widget>();
        }

        public string Id { get; }
        private string _name;
        public string Name { get { return _name; } set { Set(ref _name, value); } }

        public List<Widget> Widgets { get; set; }
    }

    public class UserData
    {
        public List<MiriotConfiguration> Devices { get; set; }

        public DateTime? PreviousLoginDate { get; set; }

        public UserEmotion PreviousEmotion { get; set; }

        public bool IsVoiceActivated { get; set; }

        public Dictionary<string, string> CachedTvUrls { get; set; }

        public Dictionary<DateTime, int> ToothbrushingHistory { get; set; }
    }
}
