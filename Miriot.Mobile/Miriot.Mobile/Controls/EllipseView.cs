using Xamarin.Forms;

namespace Miriot.Mobile.Controls
{
    public class EllipseView : View
    {
        public static readonly BindableProperty FillProperty =
            BindableProperty.Create<EllipseView, Color>(p => p.Fill, Color.Accent);

        public Color Fill
        {
            get { return (Color)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly BindableProperty StrokeProperty =
         BindableProperty.Create<EllipseView, Color>(p => p.Stroke, Color.Accent);

        public Color Stroke
        {
            get { return (Color)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly BindableProperty HorizontalOptionsProperty =
                 BindableProperty.Create<EllipseView, LayoutOptions>(p => p.HorizontalOptions, LayoutOptions.Center);

        public LayoutOptions HorizontalAlignment
        {
            get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
            set { SetValue(HorizontalOptionsProperty, value); }
        }

        public static readonly BindableProperty StrokeThicknessProperty =
                 BindableProperty.Create<EllipseView, int>(p => p.StrokeThickness, 1);

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }


    }
}
