using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class SettingsPage
    {
        public SettingsPage(User user)
        {
            InitializeComponent();
            ViewModel.User = user;
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
