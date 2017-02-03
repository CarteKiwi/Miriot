using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Miriot.Common.Model.Widgets.Horoscope;

namespace Miriot.Common.Model
{
    public class User : ObservableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public UserData UserData { get; set; }
        public byte[] Picture { get; set; }
        public string PictureLocalPath { get; set; }

        private DateTime? _previousLoginDate;

        public DateTime? PreviousLoginDate
        {
            get { return _previousLoginDate; }
            set { Set(ref _previousLoginDate, value); }
        }

        private UserEmotion _emotion;
        public UserEmotion Emotion
        {
            get { return _emotion; }
            set
            {
                Set(ref _emotion, value);

                RaisePropertyChanged();
                RaisePropertyChanged(() => FriendlyEmotion);
            }
        }

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

        public int FaceRectangleTop { get; set; }
        public int FaceRectangleLeft { get; set; }
    }

    public class UserData
    {
        public List<Widget> Widgets { get; set; }

        public DateTime? PreviousLoginDate { get; set; }

        public UserEmotion PreviousEmotion { get; set; }

        public bool IsVoiceActivated { get; set; }

        public Signs HoroscopeSign { get; set; }

        public Dictionary<string, string> CachedTvUrls { get; set; }
    }
}
