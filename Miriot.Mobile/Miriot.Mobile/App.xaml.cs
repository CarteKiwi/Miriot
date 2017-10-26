using Miriot.Core;
using Miriot.Mobile.Views;

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

            Locator = new ViewModelLocator();

            if (Device.RuntimePlatform == Device.iOS)
                MainPage = new HomePage();
            else
                MainPage = new NavigationPage(new HomePage());
        }
    }
}