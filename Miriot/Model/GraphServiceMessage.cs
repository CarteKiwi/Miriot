using GalaSoft.MvvmLight.Messaging;

namespace Miriot.Model
{
    public class GraphServiceMessage : MessageBase
    {
        public bool IsAuthenticated { get; set; }

        public GraphServiceMessage(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }
}
