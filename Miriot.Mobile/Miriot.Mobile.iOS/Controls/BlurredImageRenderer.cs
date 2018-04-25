using System;
using System.Drawing;
using Miriot.Mobile.Controls;
using Miriot.Mobile.iOS.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BlurredImage), typeof(BlurredImageRenderer))]
namespace Miriot.Mobile.iOS.Controls
{
    public class BlurredImageRenderer : ImageRenderer
    {
        public BlurredImageRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            CreateBlur();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == VisualElement.HeightProperty.PropertyName ||
                e.PropertyName == VisualElement.WidthProperty.PropertyName)
            {
                CreateBlur();
            }
        }

        private void CreateBlur()
        {
            var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
            var blurView = new UIVisualEffectView(blur)
            {
                Frame = new RectangleF(0, 0, (float)Element.Width, (float)Element.Height),
                Alpha = (nfloat)0.7
            };

            NativeView.Add(blurView);
        }
    }
}
