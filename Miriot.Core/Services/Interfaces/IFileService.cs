using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Miriot.Core.Services.Interfaces
{
    public interface IFileService
    {
        byte[] GetBytes(string filePath);

        Task<byte[]> EncodedBytes(SoftwareBitmap soft);
    }
}
