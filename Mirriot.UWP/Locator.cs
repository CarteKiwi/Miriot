using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Miriot.Core.Services.Interfaces;
using Miriot.Services;
using Miriot.Views;

namespace Miriot
{
    public class Locator
    {
        static Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Services
            if (ViewModelBase.IsInDesignModeStatic)
            {
                // No mock right now
            }
            else
            {
                var navigationService = CreateNavigationService();
                SimpleIoc.Default.Register<INavigationService>(() => navigationService);
                SimpleIoc.Default.Register<IAuthentication, Authentication>();
                SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
                SimpleIoc.Default.Register<IFileService, FileService>();
                SimpleIoc.Default.Register<IDialogService, DialogService>();
                SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            }
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure("Main", typeof(MainPage));
            navigationService.Configure("Settings", typeof(SettingsPage));

            return navigationService;
        }
    }
}
