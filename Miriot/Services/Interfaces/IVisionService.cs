using Miriot.Common.Model;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface IVisionService
    {
        Task<Scene> CreateSceneAsync(byte[] bitmap);
    }
}
