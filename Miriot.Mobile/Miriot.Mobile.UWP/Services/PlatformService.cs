using System;
using Microsoft.Toolkit.Uwp;
using Miriot.Services;

namespace Miriot.Win10.Services
{
    public class PlatformService : IPlatformService
    {
        public bool IsInternetAvailable => true; //ConnectionHelper.IsInternetAvailable;
    }
}