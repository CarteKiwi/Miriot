using Miriot.Core.Services.Interfaces;
using Miriot.Win10.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Miriot.Win10.Services
{
    public class FileService : IFileService
    {
        public byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public async Task<byte[]> EncodedBytes(byte[] bitmapArray)
        {
            if (bitmapArray == null)
                return null;

            SoftwareBitmap bitmapBgra8 = SoftwareBitmap.Convert(await bitmapArray.ToSoftwareBitmapAsync(), BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            byte[] array = null;

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            using (var ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                encoder.SetSoftwareBitmap(bitmapBgra8);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) { return new byte[0]; }

                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }
    }
}
