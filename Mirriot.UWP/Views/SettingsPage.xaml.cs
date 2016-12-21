using Microsoft.Practices.ServiceLocation;
using Miriot.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Miriot.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel Vm { get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); } }

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Vm.ActionLoaded.Execute(null);
            base.OnNavigatedTo(e);
        }
    }
}
