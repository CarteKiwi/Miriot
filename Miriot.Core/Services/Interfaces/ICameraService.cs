using System.Threading.Tasks;
using Windows.Media;

namespace Miriot.Core.Services.Interfaces
{
    public interface ICameraService
    {
        Task<VideoFrame> GetLatestFrame();
    }
}
