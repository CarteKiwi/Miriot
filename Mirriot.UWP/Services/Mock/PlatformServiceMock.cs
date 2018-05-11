using Microsoft.Toolkit.Uwp.Connectivity;
using Miriot.Services;
using Windows.Networking.Connectivity;

namespace Miriot.Win10.Services
{
    public class PlatformServiceMock : IPlatformService
    {
        public bool IsInternetAvailable
        {
            get
            {
                NetworkHelper.Instance.ConnectionInformation.UpdateConnectionInformation(NetworkInformation.GetInternetConnectionProfile());
                return NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;
            }
        }

        public string GetSystemIdentifier()
        {
            return "1";
        }
    }
}