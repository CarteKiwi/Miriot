using Miriot.Services;
using System;
using Xamarin.Forms;

namespace Miriot.Mobile.Converters
{
    public class IntToStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (WidgetStates)value;
        }
    }
}