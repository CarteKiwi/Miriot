using Miriot.Common.Model.Widgets.Horoscope;
using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class UserData
    {
        public List<Widget> Widgets { get; set; }

        public DateTime? PreviousLoginDate { get; set; }

        public UserEmotion PreviousEmotion { get; set; }

        public bool IsVoiceActivated { get; set; }

        public Signs HoroscopeSign { get; set; }

        public Dictionary<string, string> CachedTvUrls { get; set; }

        public Dictionary<DateTime, int> ToothbrushingHistory { get; set; }
    }
}
