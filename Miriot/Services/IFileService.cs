using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface IFileService
    {
        byte[] GetBytes(string filePath);

        Task<byte[]> EncodedBytes(byte[] image);
    }
}
