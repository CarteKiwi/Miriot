using Miriot.Mobile.UWP.Controls;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Miriot.Mobile.Controls.AcrylicGrid), typeof(AcrylicGridRenderer))]
namespace Miriot.Mobile.UWP.Controls
{
    public class AcrylicGridRenderer : ViewRenderer<Mobile.Controls.AcrylicGrid, Windows.UI.Xaml.Controls.Grid>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Mobile.Controls.AcrylicGrid> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            var grid = new Windows.UI.Xaml.Controls.Grid();
            grid.DataContext = Element;

            var fill = Element.TintColor;
            var convert = new ColorConverter();
            var myBrush = (SolidColorBrush)convert.Convert(fill, null, null, null);

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                AcrylicBrush acrylicBrush = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.Backdrop,
                    TintColor = myBrush.Color,
                    FallbackColor = myBrush.Color,
                    TintOpacity = Element.BlurOpacity
                };

                grid.Background = acrylicBrush;
            }
            else
            {
                grid.Background = myBrush;
            }

            this.SetNativeControl(grid);
        }
    }
}
