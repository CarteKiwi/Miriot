
using Foundation;
using GalaSoft.MvvmLight.Threading;
using ImageCircle.Forms.Plugin.iOS;
using Miriot.iOS;
using Plugin.BluetoothLE;
using UIKit;

namespace Miriot.Mobile.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private static Locator _locator;
        public static Locator Locator
        {
            get => _locator ?? (_locator = new Locator());

            set { _locator = value; }
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.Popup.Init();

            Locator = new Locator();
            CrossBleAdapter.AdapterScanner.FindAdapters();
            global::Xamarin.Forms.Forms.Init();
            ImageCircleRenderer.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
