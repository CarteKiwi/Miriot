using Microsoft.Toolkit.Uwp.Connectivity;
using Miriot.Services;

namespace Miriot.Win10.Services
{
    public class PlatformServiceMock : IPlatformService
    {
        public bool IsInternetAvailable => NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

        public string GetSystemIdentifier()
        {
            return "1";
        }
    }
}