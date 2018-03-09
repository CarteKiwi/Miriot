using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Miriot.Core.Services;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels;

namespace Miriot.Core
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<ILuisService, LuisService>();
            SimpleIoc.Default.Register<IFaceService, FaceService>();
            SimpleIoc.Default.Register<IVisionService, VisionService>();

            // View Models
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();
        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();
    }
}

