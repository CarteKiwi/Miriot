using GalaSoft.MvvmLight.Messaging;

namespace Miriot.Core.Messages
{
    public class SpeakMessage : MessageBase
    {
        public string Text { get; }

        public SpeakMessage(string text)
        {
            Text = text;
        }
    }
}
