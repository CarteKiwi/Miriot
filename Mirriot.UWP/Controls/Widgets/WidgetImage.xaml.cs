using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Controls
{
    public sealed partial class WidgetImage : WidgetBase
    {
        public WidgetImage(bool isSexy)
        {
            InitializeComponent();

            var folder = "Daugther";
            if(isSexy)
                folder = "Avengers";

            var rnd = new Random();
            var urlImg = string.Format("ms-appx:///Assets/{1}/{0}.png", rnd.Next(1, 2), folder);
            Img.Source = new BitmapImage(new Uri(urlImg, UriKind.Absolute));
        }
    }
}
