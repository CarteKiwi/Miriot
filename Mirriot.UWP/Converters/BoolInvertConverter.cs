using System;
using Windows.UI.Xaml.Data;

namespace Miriot.Converters
{
    public class BoolInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool val = (bool)value;
            return !val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool val = (bool)value;
            return !val;
        }
    }
}
