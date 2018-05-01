using Miriot.Mobile.Controls;
using Miriot.Mobile.iOS.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Miriot.Mobile.iOS.Controls
{
    public class CustomWebViewRenderer : WebViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            var view = Element as CustomWebView;
            if (view == null || NativeView == null)
            {
                return;
            }
            ScalesPageToFit = true;
        }

    }
}
