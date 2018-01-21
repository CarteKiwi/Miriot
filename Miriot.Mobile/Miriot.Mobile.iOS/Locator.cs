using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.iOS.Services;
using Miriot.Services;

namespace Miriot.iOS
{
    public class Locator
    {
        static Locator()
        {
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
        }
    }
}
