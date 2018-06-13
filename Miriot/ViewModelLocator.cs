using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;
using Miriot.Services;
using Miriot.ViewModels;

namespace Miriot.Core
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<SocketService>();
            SimpleIoc.Default.Register<RemoteService>();
            SimpleIoc.Default.Register<MiriotService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConnectViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<ProfileViewModel>();
            SimpleIoc.Default.Register<CameraViewModel>();
        }

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();
        public ConnectViewModel ConnectViewModel => SimpleIoc.Default.GetInstance<ConnectViewModel>();
        public ProfileViewModel ProfileViewModel => SimpleIoc.Default.GetInstance<ProfileViewModel>();
        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstance<SettingsViewModel>();
        public CameraViewModel CameraViewModel => SimpleIoc.Default.GetInstance<CameraViewModel>();
    }
}