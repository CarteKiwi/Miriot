using Miriot.Common.Model.Widgets;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface IGraphService
    {
        void Initialize();

        bool IsInitialized { get; set; }

        Task<bool> LoginAsync();

        Task<GraphUser> GetUserAsync();
        Task LogoutAsync();
    }
}
