using Miriot.Core.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;

namespace Miriot.Services.Mock
{
    public class FileService : IFileService
    {
        public byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public async Task<byte[]> EncodedBytes(SoftwareBitmap softwareBitmap)
        {
            var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var uri = p.Path + "/untitled.png";

            var array = GetBytes(uri);

            return array;
        }
    }
}
