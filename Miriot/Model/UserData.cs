using Miriot.Common.Model.Widgets.Horoscope;
using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class MiriotConfiguration
    {
        public MiriotConfiguration(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; set; }

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
