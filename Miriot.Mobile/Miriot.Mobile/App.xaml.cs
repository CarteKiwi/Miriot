using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Core;
using Miriot.Mobile.Services;
using Miriot.Mobile.Views;
using Miriot.Services;
using Xamarin.Forms;
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
            SimpleIoc.Default.Register<IGraphService, GraphService>();

            MainPage = new NavigationPage(new HomePage());

            nav.Initialize((NavigationPage)MainPage);
        }

        private NavigationService InitializeNavigationService()
        {
            var nav = new NavigationService();
            nav.Configure(PageKeys.Main, typeof(HomePage));
            nav.Configure(PageKeys.Profile, typeof(ProfilePage));
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            return nav;
        }
    }
}