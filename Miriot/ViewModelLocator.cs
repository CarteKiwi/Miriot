using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;

namespace Miriot.Core
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConnectViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();
        public ConnectViewModel ConnectViewModel => SimpleIoc.Default.GetInstance<ConnectViewModel>();
        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstance<SettingsViewModel>();
    }
}