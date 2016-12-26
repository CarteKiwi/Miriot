using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Miriot.Common.Model
{
    public class User : ObservableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public UserData UserData { get; set; }
        public string PictureLocalPath { get; set; }

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

        public bool IsVoiceActivated { get; set; }

        public Dictionary<string, string> CachedTvUrls { get; set; }
    }
}
