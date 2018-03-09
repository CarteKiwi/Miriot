using Xamarin.Forms;

namespace Miriot.Mobile.Controls
{
    public class AcrylicGrid : Grid
    {
        public static readonly BindableProperty BlurOpacityProperty =
            BindableProperty.Create<AcrylicGrid, double>(p => p.BlurOpacity, 1.0);

        public double BlurOpacity
        {
            get { return (double)GetValue(BlurOpacityProperty); }
            set { SetValue(BlurOpacityProperty, value); }
        }

        public static readonly BindableProperty TintColorProperty =
            BindableProperty.Create<AcrylicGrid, Color>(p => p.TintColor, Color.Accent);

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }
    }
}
