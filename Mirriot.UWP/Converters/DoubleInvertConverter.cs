using System;
using Windows.UI.Xaml.Data;

namespace Miriot.Win10.Converters
{
    public class DoubleInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double result;
            if (value != null && double.TryParse(value.ToString(), out result))
            {
                return result * -1;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
