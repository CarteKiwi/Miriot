using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Core;
using Miriot.Mobile.Views;

using Xamarin.Forms;
using Xamarin.Forms.Alias;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Miriot.Mobile
{
    public partial class App : Application
    {
        private static ViewModelLocator _locator;
        public static ViewModelLocator Locator
        {
            get => _locator ?? (_locator = new ViewModelLocator());

            set { _locator = value; }
        }

        public App()
        {
            InitializeComponent();

            var nav = InitializeNavigationService();

            Locator = new ViewModelLocator();
            
            Alias.Init();

            if (Device.RuntimePlatform == Device.iOS)
                MainPage = new HomePage();
            else
                MainPage = new NavigationPage(new HomePage());

            nav.Initialize((NavigationPage)MainPage);
        }

        private NavigationService InitializeNavigationService()
        {
            var nav = new NavigationService();
            nav.Configure(PageKeys.Main, typeof(HomePage));
            nav.Configure(PageKeys.Profile, typeof(ProfilePage));
            nav.Configure(PageKeys.Settings, typeof(SettingsPage));
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            return nav;
        }
    }
}