using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.iOS.Services;
using Miriot.Services;

namespace Miriot.iOS
{
    public class Locator
    {
        public Locator()
        {
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IConfigurationService, ConfigurationService>();
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
#if MOCK
            SimpleIoc.Default.Register<IBluetoothService, BluetoothClientServiceMock>();
#else
            SimpleIoc.Default.Register<IBluetoothService, BluetoothClientService>();
#endif
            SimpleIoc.Default.Register<ITwitterService, TwitterService>();
        }
    }
}
