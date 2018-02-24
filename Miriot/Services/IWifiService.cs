using Miriot.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IWifiService
    {
        Task<IEnumerable<WifiNetwork>> GetWifiAsync();

        Task ConnectWifiAsync(string bssid, string pwd);
    }
}
