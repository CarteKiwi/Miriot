using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;
using Miriot.Services;

namespace Miriot.Core
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<SocketService>();
            SimpleIoc.Default.Register<RemoteService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConnectViewModel>();
            SimpleIoc.Default.Register<ProfileViewModel>();
        }

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();
        public ConnectViewModel ConnectViewModel => SimpleIoc.Default.GetInstance<ConnectViewModel>();
        public ProfileViewModel ProfileViewModel => SimpleIoc.Default.GetInstance<ProfileViewModel>();
    }
}