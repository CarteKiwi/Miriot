using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Win10.Utils
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
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);

            byte[] pixels = new byte[fileStream.Size];

            reader.ReadBytes(pixels);

            return pixels;
        }

        public static async Task<string> ToFile(this SoftwareBitmap softwareBitmap)
        {
            SoftwareBitmap bitmapBgra8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var file = await Package.Current.InstalledLocation.CreateFileAsync("Miriot.Win10.jpg", CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(bitmapBgra8);

                await encoder.FlushAsync();
            }

            return file.Path;
        }

        public static async Task<SoftwareBitmap> ToSoftwareBitmap(this byte[] bytes)
        {
            var stream = bytes.AsBuffer().AsStream();
            var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.PngDecoderId, stream.AsRandomAccessStream());
            return await decoder.GetSoftwareBitmapAsync();
        }
    }
}
