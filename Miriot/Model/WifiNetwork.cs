using System;

namespace Miriot.Common.Model
{
    public class WifiNetwork
    {
        public bool IsSecure { get; set; }
        public String Ssid { get; set; }
        public String Bssid { get; set; }
        public byte SignalBars { get; set; }
    }
}
