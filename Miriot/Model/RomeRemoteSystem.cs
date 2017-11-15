using System.Net;

namespace Miriot.Model
{
    public class RomeRemoteSystem
    {
        public RomeRemoteSystem(object nativeObject)
        {
            NativeObject = nativeObject;
        }

        public object NativeObject { get; }

        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
        public string Kind { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public string DisplayMember => $"{DisplayName} - {Status}";
    }
}
