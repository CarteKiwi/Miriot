using System;
using Windows.UI.Xaml;
using Xamarin.Forms;

namespace Miriot.Mobile.UWP.Controls
{
    public class HorizontalOptionsConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var option = (LayoutOptions)value;

            if (option.Alignment == LayoutOptions.Center.Alignment)
            {
                return HorizontalAlignment.Center;
            }
            else
            {
                return HorizontalAlignment.Left;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
