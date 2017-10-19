using Miriot.Core.Services.Interfaces;
using System;
using Windows.UI.Xaml.Data;

namespace Miriot.Win10.Converters
{
    public class WidgetStateToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean into a Visibility. Visible when the original value is true.
        /// To reverse the behavior, set "false" as ConverterParameter
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (WidgetStates?)Enum.Parse(typeof(WidgetStates), parameter.ToString()) == (WidgetStates)Enum.Parse(typeof(WidgetStates), value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
