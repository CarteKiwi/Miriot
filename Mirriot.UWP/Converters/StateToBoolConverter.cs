using System;
using Windows.UI.Xaml.Data;
using Miriot.Common;

namespace Miriot.Converters
{
    public class StateToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean into a Visibility. Visible when the original value is true.
        /// To reverse the behavior, set "false" as ConverterParameter
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (States?)Enum.Parse(typeof(States), parameter.ToString()) == (States)Enum.Parse(typeof(States), value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
