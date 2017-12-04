using Microsoft.Toolkit.Uwp;
using Miriot.Services;
using System;
using Windows.System.Profile;

namespace Miriot.Win10.Services
{
    public class PlatformService : IPlatformService
    {
        public bool IsInternetAvailable => NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

        public string GetSystemIdentifier()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

                byte[] bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", "");
            }

            throw new Exception("NO API FOR DEVICE ID PRESENT!");
        }
    }
}