using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Model;
using Miriot.Services;
using Miriot.Win10.Controls;
using Miriot.Win10.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            Camera.Initialized = Handler;

            base.OnNavigatedTo(e);
        }

        private void Handler()
        {
            ExpositionController.Maximum = Camera.MaximumExposure;
            ExpositionController.Minimum = Camera.MinimumExposure;

            ZoomController.Maximum = Camera.MaximumZoom;
            ZoomController.Minimum = Camera.MinimumZoom;

            WhiteController.Maximum = Camera.Controller.WhiteBalance.Capabilities.Max;
            WhiteController.Minimum = Camera.Controller.WhiteBalance.Capabilities.Min;

            BrightnessController.Maximum = Camera.Controller.Brightness.Capabilities.Max;
            BrightnessController.Minimum = Camera.Controller.Brightness.Capabilities.Min;

            ContrastController.Maximum = Camera.Controller.Contrast.Capabilities.Max;
            ContrastController.Minimum = Camera.Controller.Contrast.Capabilities.Min;

            FocusController.Maximum = Camera.Controller.Focus.Capabilities.Max;
            FocusController.Minimum = Camera.Controller.Focus.Capabilities.Min;

            PopulateSettingsComboBox();

            //SetSliders();
        }

        private void SetSliders()
        {
            Camera.Controller.WhiteBalance.TryGetValue(out var white);
            WhiteController.Value = white;
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

        private void ContrastController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustContrast(e.NewValue);
        }

        private void WhiteController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustWhite(e.NewValue);
        }

        private void FocusController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustFocus(e.NewValue);
        }

        private void ZoomController_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Camera.AdjustZoom(e.NewValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo(PageKeys.Main);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Camera.PersistSettings();
        }

        /// <summary>
        ///  Event handler for Preview settings combo box. Updates stream resolution based on the selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ComboBoxSettings_Changed(object sender, RoutedEventArgs e)
        {
            if (Camera._isPreviewing)
            {
                var selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
                var res = (selectedItem.Tag as StreamResolution);
                var encodingProperties = res.EncodingProperties;
                await Camera.Controller.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                Camera.FriendlyResolution = res.GetFriendlyName();
            }
        }

        /// <summary>
        /// Populates the combo box with all possible combinations of settings returned by the camera driver
        /// </summary>
        private void PopulateSettingsComboBox()
        {
            // Query all properties of the device
            IEnumerable<StreamResolution> allProperties = Camera.Controller.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));

            // Order them by resolution then frame rate
            allProperties = allProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            // Populate the combo box with the entries
            foreach (var property in allProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName();
                comboBoxItem.Tag = property;
                CameraSettings.Items.Add(comboBoxItem);
            }
        }
    }
}
