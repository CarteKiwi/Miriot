using Miriot.Win10.Utils;
using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Miriot.Mobile.UWP.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var toto = ((byte[]) value)?.AsBitmapImage();

            return new ImageBrush() { ImageSource = toto };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
