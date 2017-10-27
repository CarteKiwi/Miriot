using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Services;
using Miriot.Droid.Services;
using Miriot.Mobile.Droid.Services;
using Miriot.Mobile.Views;
using Miriot.Services.Interfaces;

namespace Miriot.Droid
{
    public class Locator
    {
        static Locator()
        {
            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register(() => navigationService);
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IRomeService, RomeService>();
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(PageKeys.Connect, typeof(HomePage));

            return navigationService;
        }
    }
}
