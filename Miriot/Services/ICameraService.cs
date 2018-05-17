using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface ICameraService
    {
        Task<object> GetLatestFrame();
        Task<byte[]> GetEncodedBytesAsync(object videoFrame);
        void AdjustBrightness(double value);
        void AdjustExposition(double value);
        void AdjustContrast(double value);
        bool ShowPreview { get; set; }
        void PersistSettings();
    }
}
