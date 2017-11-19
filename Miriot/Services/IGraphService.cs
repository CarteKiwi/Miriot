using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IGraphService
    {
        void Initialize();

        bool IsInitialized { get; set; }

        Task<bool> LoginAsync(bool hideError = false);

        Task<GraphUser> GetUserAsync();

        Task LogoutAsync();

        Task<string> GetCodeAsync();

        Task AuthenticateForDeviceAsync();
    }
}
