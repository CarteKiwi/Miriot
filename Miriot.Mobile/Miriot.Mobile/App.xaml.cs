using Miriot.Standard.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Miriot.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
                MainPage = new HomePage();
            else
                MainPage = new NavigationPage(new HomePage());
        }
    }
}