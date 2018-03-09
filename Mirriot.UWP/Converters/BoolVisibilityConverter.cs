using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Miriot.Win10.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean into a Visibility. Visible when the original value is true.
        /// To reverse the behavior, set "false" as ConverterParameter
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool mode = (parameter as string) != "false";
            bool val = (bool)value;

            if (mode == val)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;

            return false;
        }
    }
}
