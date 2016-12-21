using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Miriot.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double dble;
            if (double.TryParse(value?.ToString().Replace('.', ','), out dble))
            {
                return string.Format(parameter as string, dble.ToString("N0", CultureInfo.CurrentUICulture));
            }

            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
