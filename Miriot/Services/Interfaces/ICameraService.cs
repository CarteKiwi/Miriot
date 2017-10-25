using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface ICameraService
    {
        Task<object> GetLatestFrame();
    }
}
