using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Miriot.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a nullable object into a Visibility.
        /// If the object is null, return Collapsed
        /// To reverse the behavior, set "false" as ConverterParameter
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return parameter == null ? Visibility.Collapsed : Visibility.Visible;

            return parameter == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
