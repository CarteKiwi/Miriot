using Miriot.Common.Model;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IVisionService
    {
        Task<Scene> CreateSceneAsync(byte[] bitmap);
    }
}
