using Miriot.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IPlatformService
    {
        bool IsInternetAvailable { get; }

        string GetSystemIdentifier();
    }
}