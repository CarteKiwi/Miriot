using System.Collections.Generic;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IConfigurationService
    {
        Task<Dictionary<string, string>> GetKeysByProviderAsync(string providerName);
    }
}
