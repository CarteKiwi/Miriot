using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<Dictionary<string, string>> GetKeysByProviderAsync(string providerName);
    }
}
