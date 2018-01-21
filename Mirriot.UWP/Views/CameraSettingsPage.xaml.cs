using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Miriot.Win10.Views
{
    public sealed partial class CameraSettingsPage : Page
    {
        public CameraSettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var remoteService = SimpleIoc.Default.GetInstance<RemoteService>();

            remoteService.CommandReceived = OnCommandReceivedAsync;
            base.OnNavigatedTo(e);
        }

        private Task<string> OnCommandReceivedAsync(RemoteParameter parameter)
        {
            switch (parameter.Command)
            {
                case RemoteCommands.CameraAdjustBrightness:

                    break;
                case RemoteCommands.GoToCameraPage:
                    SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo(PageKeys.Main);
                    return null;
            }

            return Task.FromResult(string.Empty);
        }
    }
}
