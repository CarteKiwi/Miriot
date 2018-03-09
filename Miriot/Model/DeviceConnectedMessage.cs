using GalaSoft.MvvmLight.Messaging;

namespace Miriot.Model
{
    public class DeviceConnectedMessage : MessageBase
    {
        public string Name { get; set; }

        public DeviceConnectedMessage(string name)
        {
            Name = name;
        }
    }
}
