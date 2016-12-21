using Miriot.Core.Services.Interfaces;
using System.IO;

namespace Miriot.Services
{
    public class FileService : IFileService
    {
        public byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
