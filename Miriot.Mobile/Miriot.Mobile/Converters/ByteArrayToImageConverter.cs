using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace Miriot.Mobile.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource imgsrc = null;
            try
            {
                if (value == null)
                    return null;
                byte[] bArray = (byte[])value;

                imgsrc = ImageSource.FromStream(() =>
                {
                    var ms = new MemoryStream(bArray);
                    ms.Position = 0;
                    return ms;
                });
            }
            catch (System.Exception sysExc)
            {
                System.Diagnostics.Debug.WriteLine(sysExc.Message);
            }
            return imgsrc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
