using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Utils
{
    public static class ImageHelper
    {
        //public static async Task<WriteableBitmap> GetLocalImage(this string path)
        //{
        //    var file = await Package.Current.InstalledLocation.GetFileAsync(path);
        //    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        //    {
        //        var wbm = new BitmapImage(156, 156);
        //        wbm.SetSource(stream);
        //        return wbm;
        //    }
        //}

        public static async Task<BitmapImage> AsBitmapImageAsync(this IBuffer buffer)
        {
            if (buffer != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(buffer);
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    return image;
                }
            }
            return null;
        }

        public static async Task<BitmapImage> AsBitmapImageAsync(this byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(byteArray.AsBuffer());
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    return image;
                }
            }
            return null;
        }

        public static BitmapImage AsBitmapImage(this byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream().AsStreamForWrite())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                    var image = new BitmapImage();
                    stream.Seek(0,0);
                    image.SetSource(stream.AsRandomAccessStream());
                    return image;
                }
            }
            return null;
        }

        public static async Task<byte[]> ToBytes(this StorageFile image)
        {
            IRandomAccessStream fileStream = await image.OpenAsync(FileAccessMode.Read);
            var reader = new Windows.Storage.Streams.DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);

            byte[] pixels = new byte[fileStream.Size];

            reader.ReadBytes(pixels);

            return pixels;
        }
    }
}
