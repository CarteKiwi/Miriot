using Miriot.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage
    {
        public ProfilePage(MiriotParameter parameter)
        {
            InitializeComponent();
            ViewModel.SetParameters(parameter);
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}