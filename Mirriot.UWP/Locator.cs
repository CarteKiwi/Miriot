using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Services;
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
#if MOCK
            SimpleIoc.Default.Register<ISpeechService, FakeSpeechService>();
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Services.Mock.FrameAnalyser<ServiceResponse>>();
#else
            SimpleIoc.Default.Register<ISpeechService, SpeechService>();
            SimpleIoc.Default.Register<IFrameAnalyzer<ServiceResponse>, Utils.FrameAnalyser<ServiceResponse>>();
#endif
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IBluetoothService, BluetoothService>();
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(PageKeys.Main, typeof(MainPage));
            navigationService.Configure(PageKeys.CameraSettings, typeof(CameraSettingsPage));

            return navigationService;
        }
    }
}
