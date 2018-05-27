﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Services;
using Miriot.Win10.Controls;
using Miriot.Win10.Services;
using Miriot.Win10.Views;
using Windows.Media;

namespace Miriot.Win10
{
    public class Locator
    {
        static Locator()
        {
            Cognitive.Bootstrap.Load();

            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register(() => navigationService);
            SimpleIoc.Default.Register<IConfigurationService, ConfigurationService>();
            SimpleIoc.Default.Register<IGraphService, GraphService>();
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<ITwitterService, TwitterWrapperService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
#if MOCK
            SimpleIoc.Default.Register<IPlatformService, PlatformServiceMock>();
            SimpleIoc.Default.Register<ISpeechService, FakeSpeechService>();
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Services.Mock.FrameAnalyser<ServiceResponse>>();
            SimpleIoc.Default.Register<IBluetoothService, BluetoothService>();
#else
            SimpleIoc.Default.Register<ISpeechService, SpeechService>();
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Utils.FrameAnalyser<ServiceResponse>>();
            SimpleIoc.Default.Register<IPlatformService, PlatformServiceMock>();
            SimpleIoc.Default.Register<IBluetoothService, BluetoothService>();
#endif
            SimpleIoc.Default.Register<IWifiService, WifiService>();
            SimpleIoc.Default.Register<ICameraService>(() => new CameraControl());
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(PageKeys.Main, typeof(MainPage));
            navigationService.Configure(PageKeys.WifiSettings, typeof(WifiSettingsPage));
            navigationService.Configure(PageKeys.CameraSettings, typeof(CameraSettingsPage));

            return navigationService;
        }
    }
}
