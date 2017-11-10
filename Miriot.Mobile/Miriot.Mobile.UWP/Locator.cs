using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Mobile.UWP.Services;
using Miriot.Services;
using Miriot.Services.Interfaces;
using Miriot.Win10.Services;

namespace Miriot.Win10
{
    public class Locator
    {
        static Locator()
        {
            SimpleIoc.Default.Register<IConfigurationService, ConfigurationService>();
            SimpleIoc.Default.Register<IDispatcherService, DispatcherService>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<IGraphService, GraphService>();
            SimpleIoc.Default.Register<ISecurityService, SecurityService>();
            SimpleIoc.Default.Register<ITwitterService, TwitterWrapperService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IRomeService, RomeService>();
        }
    }
}
