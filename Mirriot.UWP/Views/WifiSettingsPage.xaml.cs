using Miriot.Common.Model;
using Miriot.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Miriot.Win10.Views
{
    public sealed partial class WifiSettingsPage : Page
    {
        public SettingsViewModel Vm => DataContext as SettingsViewModel;

        public WifiSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Vm.Initialize();
            base.OnNavigatedTo(e);
        } 
    }
}
