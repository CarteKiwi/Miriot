using Miriot.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class SettingsPage
    {
        public SettingsPage(MiriotParameter parameter)
        {
            InitializeComponent();
            ViewModel.SetParameters(parameter);
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
