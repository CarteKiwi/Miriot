﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Miriot.Common;
using Miriot.Controls;
using Miriot.Core.Services.Interfaces;
using Miriot.Services;
using Miriot.Services.Mock;
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
                SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Services.Mock.FrameAnalyser<ServiceResponse>>();
                //SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, FrameAnalyser<ServiceResponse>>();
                SimpleIoc.Default.Register<IFileService, FileService>();
                SimpleIoc.Default.Register<IDialogService, DialogService>();
                SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            }
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
