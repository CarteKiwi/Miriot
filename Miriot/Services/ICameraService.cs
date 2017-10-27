using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface ICameraService
    {
        Task<object> GetLatestFrame();
    }
}
