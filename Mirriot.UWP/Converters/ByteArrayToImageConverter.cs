using Miriot.Utils;
using System;
using Windows.UI.Xaml.Data;

namespace Miriot.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((byte[]) value)?.AsBitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
