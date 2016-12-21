using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public UserData UserData { get; set; }
        public string PictureLocalPath { get; set; }
        public UserEmotion Emotion { get; set; }
        public string FriendlyEmotion
        {
            get
            {
                switch (Emotion)
                {
                    case UserEmotion.Happiness:
                        return "heureux";
                    case UserEmotion.Surprise:
                        return "étonné";
                    case UserEmotion.Sadness:
                        return "triste";
                    case UserEmotion.Fear:
                        return "apeuré";
                    case UserEmotion.Anger:
                        return "énervé";
                }

                return null;
            }
        }
    }

    public class UserData
    {
        public List<Widget> Widgets { get; set; }

        public bool IsVoiceActivated { get; set; }

        public Dictionary<string, string> CachedTvUrls { get; set; }
    }
}
