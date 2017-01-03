using Microsoft.Practices.ServiceLocation;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Windows.UI.Xaml.Navigation;

namespace Miriot.Views
{
    public sealed partial class SettingsPage
    {
        public SettingsViewModel Vm { get; } = ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                Vm.User = e.Parameter as User;
            }

            Vm.ActionLoaded.Execute(null);
            base.OnNavigatedTo(e);
        }
    }
}
