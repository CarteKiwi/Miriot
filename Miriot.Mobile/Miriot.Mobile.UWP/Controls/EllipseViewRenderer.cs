using System.ComponentModel;
using Miriot.Mobile.Controls;
using Miriot.Mobile.UWP.Controls;
using Miriot.Mobile.UWP.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Media;

[assembly: ExportRenderer(typeof(EllipseView), typeof(EllipseViewRenderer))]
namespace Miriot.Mobile.UWP.Controls
{
    public class EllipseViewRenderer : ViewRenderer<EllipseView, Ellipse>
    {
        private Ellipse _ellipse;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Image")
            {
                var converter = new ByteArrayToImageConverter();
                var value = converter.Convert(((EllipseView)sender).Image, null, null, null);
                _ellipse.Fill = (ImageBrush)value;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<EllipseView> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            _ellipse = new Ellipse();
            _ellipse.DataContext = Element;
            _ellipse.SetBinding(Ellipse.FillProperty, new Binding() { Path = new PropertyPath("Fill"), Converter = new ColorConverter() });
            _ellipse.SetBinding(Ellipse.StrokeProperty, new Binding() { Path = new PropertyPath("Stroke"), Converter = new ColorConverter() });
            _ellipse.SetBinding(Ellipse.HorizontalAlignmentProperty, new Binding() { Path = new PropertyPath("HorizontalOptions"), Converter = new HorizontalOptionsConverter() });
            _ellipse.SetBinding(Ellipse.StrokeThicknessProperty, new Binding() { Path = new PropertyPath("StrokeThickness") });

            this.SetNativeControl(_ellipse);
        }
    }
}
