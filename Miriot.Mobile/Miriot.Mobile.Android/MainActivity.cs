
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net.Wifi;
using Android.OS;
using GalaSoft.MvvmLight.Threading;
using Miriot.Droid;
using static Android.Net.Wifi.WifiManager;

namespace Miriot.Mobile.Droid
{
    [Activity(Label = "Miriot.Mobile.Android", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private static Locator _locator;
        public static Locator Locator
        {
            get => _locator ?? (_locator = new Locator());

            set { _locator = value; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            DispatcherHelper.Initialize();

            Locator = new Locator();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}