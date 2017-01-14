using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Miriot.Common;
using Miriot.Controls;
using Miriot.Core.Services.Interfaces;
using Miriot.Services;
using Miriot.Utils;
using Miriot.Views;

namespace Miriot
{
    public class Locator
    {
        static Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            SimpleIoc.Default.Register<IAuthentication, Authentication>();
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
#if MOCK
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Services.Mock.FrameAnalyser<ServiceResponse>>();
#else
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, FrameAnalyser<ServiceResponse>>();
#endif
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(PageKeys.Main, typeof(MainPage));
            navigationService.Configure(PageKeys.Settings, typeof(SettingsPage));

            return navigationService;
        }
    }
}
