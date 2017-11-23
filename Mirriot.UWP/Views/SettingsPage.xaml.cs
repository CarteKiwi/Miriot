using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Miriot.Win10.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel Vm { get; } = SimpleIoc.Default.GetInstance<SettingsViewModel>();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                //Vm.MiriotParameter = e.Parameter as MiriotParameter;
            }
            Vm.Initialize();
            base.OnNavigatedTo(e);
        }
    }
}
