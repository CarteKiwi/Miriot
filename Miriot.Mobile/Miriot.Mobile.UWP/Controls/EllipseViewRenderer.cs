using Miriot.Mobile.Controls;
using Miriot.Mobile.UWP.Controls;
using Miriot.Mobile.UWP.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(EllipseView), typeof(EllipseViewRenderer))]
namespace Miriot.Mobile.UWP.Controls
{
    public class EllipseViewRenderer : ViewRenderer<EllipseView, Ellipse>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<EllipseView> e)
        {
            base.OnElementChanged(e);

            var ellipse = new Ellipse();
            ellipse.DataContext = Element;
            if (e.NewElement?.Image != null)
                ellipse.SetBinding(Ellipse.FillProperty, new Binding() { Path = new PropertyPath("Image"), Converter = new ByteArrayToImageConverter() });
            else
                ellipse.SetBinding(Ellipse.FillProperty, new Binding() { Path = new PropertyPath("Fill"), Converter = new ColorConverter() });
            ellipse.SetBinding(Ellipse.StrokeProperty, new Binding() { Path = new PropertyPath("Stroke"), Converter = new ColorConverter() });
            ellipse.SetBinding(Ellipse.HorizontalAlignmentProperty, new Binding() { Path = new PropertyPath("HorizontalOptions"), Converter = new HorizontalOptionsConverter() });
            ellipse.SetBinding(Ellipse.StrokeThicknessProperty, new Binding() { Path = new PropertyPath("StrokeThickness") });

            this.SetNativeControl(ellipse);
        }
    }
}
