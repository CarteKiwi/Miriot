using Miriot.Common.Model;
using Miriot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;

namespace Miriot.Win10.Services
{
    public class WifiService : IWifiService
    {
        private IList<WiFiNetworkDisplay> _wifisOriginals;
        private IList<WifiNetwork> _wifis;
        private WiFiAdapter _firstAdapter;

        public async Task ConnectWifiAsync(string bssid, string pwd)
        {
            var selectedNetwork = _wifisOriginals.First(w => w.Bssid == bssid);
            if (selectedNetwork == null || _firstAdapter == null)
            {
                //Network not selected"
                return;
            }

            WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic;

            WiFiConnectionResult result;
            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                selectedNetwork.AvailableNetwork.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None)
            {
                result = await _firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind);
            }
            else
            {
                // Only the password portion of the credential need to be supplied
                var credential = new PasswordCredential();

                // Make sure Credential.Password property is not set to an empty string. 
                // Otherwise, a System.ArgumentException will be thrown.
                // The default empty password string will still be passed to the ConnectAsync method,
                // which should return an "InvalidCredential" error
                //if (!string.IsNullOrEmpty(NetworkKey.Password))
                //{
                //    credential.Password = NetworkKey.Password;
                //}

                result = await _firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
            }

            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                //string.Format("Successfully connected to {0}.", selectedNetwork.Ssid)
            }
            else
            {
                //string.Format("Could not connect to {0}. Error: {1}", selectedNetwork.Ssid, result.ConnectionStatus)
            }
        }

        public async Task<IEnumerable<WifiNetwork>> GetWifiAsync()
        {
            _wifis = new List<WifiNetwork>();
            _wifisOriginals = new List<WiFiNetworkDisplay>();

            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                //rootPage.NotifyUser("Access denied", NotifyType.ErrorMessage);
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());

                if (result.Count >= 1)
                {
                    _firstAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                    await ScanAsync();

                    return _wifis;
                }
                else
                {
                    //No WiFi Adapters detected on this machine
                }
            }

            return null;
        }

        private async Task ScanAsync()
        {
            await _firstAdapter.ScanAsync();
            await DisplayNetworkReportAsync(_firstAdapter.NetworkReport);
        }

        private async Task DisplayNetworkReportAsync(WiFiNetworkReport report)
        {
            _wifisOriginals.Clear();
            _wifis.Clear();

            foreach (var network in report.AvailableNetworks)
            {
                var networkDisplay = new WiFiNetworkDisplay(network, _firstAdapter);
                await networkDisplay.UpdateConnectivityLevel();

                _wifisOriginals.Add(networkDisplay);
                _wifis.Add(new WifiNetwork()
                {
                    Bssid = networkDisplay.Bssid,
                    Ssid = network.Ssid,
                    IsSecure = network.SecuritySettings.NetworkAuthenticationType != NetworkAuthenticationType.Open80211
                });
            }
        }
    }
}
