using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface IFileService
    {
        byte[] GetBytes(string filePath);

        Task<byte[]> EncodedBytes(byte[] image);
    }
}
