using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Core.Services.Interfaces;
using Miriot.Mobile.UWP;
using Miriot.Mobile.UWP.Services;
using Miriot.Mobile.Views;
using Miriot.Services.Interfaces;
using Miriot.Win10.Services;
using Miriot.Win10.Utils;

namespace Miriot.Win10
{
    public class Locator
    {
        static Locator()
        {
            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register(() => navigationService);
            SimpleIoc.Default.Register<IConfigurationService, ConfigurationService>();
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<ITwitterService, TwitterWrapperService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IRomeService, RomeService>();
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(PageKeys.Main, typeof(MainPage));
            navigationService.Configure(PageKeys.Connect, typeof(HomePage));

            return navigationService;
        }
    }
}
