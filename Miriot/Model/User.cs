using GalaSoft.MvvmLight;
using System;
using System.Drawing;

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

        public Rectangle FaceRectangle { get; set; }
    }
}
