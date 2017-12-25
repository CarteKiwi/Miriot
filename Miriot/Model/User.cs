using GalaSoft.MvvmLight;
using Miriot.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Miriot.Common.Model
{
    public class User : ObservableObject
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<MiriotConfiguration> Devices { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public byte[] Picture { get; set; }

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

        public List<ToothbrushingEntry> ToothbrushingHistory { get; set; }

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

        public Rectangle FaceRectangle { get; set; }
    }
}
