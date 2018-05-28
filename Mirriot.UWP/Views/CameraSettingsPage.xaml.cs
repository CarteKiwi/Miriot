﻿using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Miriot.Win10.Controls;
using Newtonsoft.Json;
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
        private Func<RemoteParameter, Task<string>> _functionBackup;

        public CameraSettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var remoteService = SimpleIoc.Default.GetInstance<RemoteService>();
            _functionBackup = remoteService.CommandReceived;

            remoteService.CommandReceived = OnCommandReceivedAsync;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            (Camera as CameraControl).Cleanup();
            Camera = null;
            base.OnNavigatedFrom(e);
        }

        private Task<string> OnCommandReceivedAsync(RemoteParameter parameter)
        {
            var _dispatcherService = SimpleIoc.Default.GetInstance<IDispatcherService>();

            switch (parameter.Command)
            {
                case RemoteCommands.CameraPreview:
                    _dispatcherService.Invoke(() =>
                    {
                        var showPreview = JsonConvert.DeserializeObject<bool>(parameter.SerializedData);
                        Camera.ShowPreview = showPreview;
                    });
                    return null;
                case RemoteCommands.CameraAdjustBrightness:
                    _dispatcherService.Invoke(() =>
                    {
                        var value = JsonConvert.DeserializeObject<double>(parameter.SerializedData);
                        Camera.AdjustBrightness(value);
                    });
                    return null;
                case RemoteCommands.CameraPersist:
                    Camera.PersistSettings();
                    return null;
                default:
                    var remoteService = SimpleIoc.Default.GetInstance<RemoteService>();
                    remoteService.CommandReceived = _functionBackup;
                    SimpleIoc.Default.GetInstance<INavigationService>().GoBack();
                    return null;
            }
        }

        private void BrightnessController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustBrightness(e.NewValue);
        }

        private void ExpositionController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustExposition(e.NewValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo(PageKeys.Main);
        }
    }
}
