using System;
using Microsoft.Toolkit.Uwp;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Services
{
    public class PlatformService : IPlatformService
    {
        public bool IsInternetAvailable => ConnectionHelper.IsInternetAvailable;
    }
}