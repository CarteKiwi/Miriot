using Miriot.Services;
using Miriot.Win10.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Devices = Windows.Devices;

namespace Miriot.Win10.Controls
{
    public sealed partial class CameraControl : UserControl, ICameraService
    {
        #region Variables
        private MediaCapture _mediaCapture;
        private bool _isInitialized;
        private bool _isPreviewing;

        // Information about the camera device
        private bool _mirroringPreview;
        private string _cameraId;

        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls       
        private DisplayInformation _displayInformation;
        private readonly SimpleOrientationSensor _orientationSensor = SimpleOrientationSensor.GetDefault();
        private SimpleOrientation _deviceOrientation = SimpleOrientation.NotRotated;
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;

        private AdvancedPhotoCapture _advancedCapture;
        private bool _lowLightSupported;
        #endregion

        #region Dependency Properties
        public bool OptimizeForLowLight
        {
            get { return (bool)GetValue(OptimizeForLowLightProperty); }
            set { SetValue(OptimizeForLowLightProperty, value); }
        }

        public static readonly DependencyProperty OptimizeForLowLightProperty =
            DependencyProperty.Register("OptimizeForLowLight", typeof(bool), typeof(CameraControl), new PropertyMetadata(false));

        public bool ShowPreview
        {
            get { return (bool)GetValue(ShowPreviewProperty); }
            set { SetValue(ShowPreviewProperty, value); }
        }

        public static readonly DependencyProperty ShowPreviewProperty =
            DependencyProperty.Register("ShowPreview", typeof(bool), typeof(CameraControl), new PropertyMetadata(false, OnPreviewChanged));

        private static void OnPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CameraControl)d).PreviewControl.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        public Action Initialized { get; set; }
        public double MaximumExposure => _mediaCapture.VideoDeviceController.Exposure.Capabilities.Max;
        public double MinimumExposure => _mediaCapture.VideoDeviceController.Exposure.Capabilities.Min;

        public CameraControl()
        {
            InitializeComponent();

            Loaded += async (a, b) =>
            {
                _displayInformation = DisplayInformation.GetForCurrentView();
                await InitializeCameraAsync();
            };
        }

        #region Methods
        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, starts preview and unlocks the UI
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += MediaCapture_Failed;

                // Initialize MediaCapture
                try
                {
                    var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                    if (cameraDevices == null)
                    {
                        Debug.WriteLine("No camera device found!");
                        return;
                    }

                    foreach (var device in cameraDevices)
                    {
                        if (device.EnclosureLocation == null)
                        {
                            _cameraId = device.Id;
                            break;
                        }

                        switch (device.EnclosureLocation.Panel)
                        {
                            default:
                                _cameraId = device.Id;
                                break;
                            case Devices.Enumeration.Panel.Back:
                                break;
                        }
                    }

                    await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = _cameraId });

                    _isInitialized = true;
                    Initialized?.Invoke();
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception when initializing MediaCapture with {0}: {1}", _cameraId, ex);
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    // Only mirror the preview if the camera is on the front panel
                    _mirroringPreview = true;

                    if (OptimizeForLowLight)
                    {
                        await InitializeForLowLight();
                    }

                    PreviewControl.Source = _mediaCapture;
                    RegisterOrientationEventHandlers();
                    await StartPreviewAsync();

                    AdjustSettings();
                }
            }
        }

        private async Task InitializeForLowLight()
        {
            _lowLightSupported =
                _mediaCapture.VideoDeviceController.AdvancedPhotoControl.SupportedModes.Contains(Windows.Media.Devices.AdvancedPhotoMode.LowLight);

            _mediaCapture.VideoDeviceController.DesiredOptimization = MediaCaptureOptimization.Quality;

            if (_lowLightSupported)
            {
                // Choose LowLight mode
                var settings = new AdvancedPhotoCaptureSettings { Mode = AdvancedPhotoMode.LowLight };
                _mediaCapture.VideoDeviceController.AdvancedPhotoControl.Configure(settings);

                // Prepare for an advanced capture
                _advancedCapture =
                    await _mediaCapture.PrepareAdvancedPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Nv12));
            }
        }

        private void AdjustSettings()
        {
            _mediaCapture.VideoDeviceController.DesiredOptimization = MediaCaptureOptimization.Quality;
            //var focus = _mediaCapture.VideoDeviceController.;

            //if (focus.Capabilities.Supported)
            //{
            //    if (focus.Capabilities.AutoModeSupported)
            //    {
            //        focus.TrySetAuto(false);
            //    }

            //    focus.TrySetValue(focus.Capabilities.Min);
            //}

            var contrast = ApplicationData.Current.LocalSettings.Values["CameraContrast"];
            var brightness = ApplicationData.Current.LocalSettings.Values["CameraBrightness"];
            var exposure = ApplicationData.Current.LocalSettings.Values["CameraExposure"];

            if (contrast != null)
                AdjustContrast((double)contrast);
            if (brightness != null)
                AdjustBrightness((double)brightness);
            if (exposure != null)
                AdjustExposition((double)exposure);
        }

        public void AdjustContrast(double value)
        {
            var contrast = _mediaCapture.VideoDeviceController.Contrast;

            if (contrast.Capabilities.Supported)
            {
                if (contrast.Capabilities.AutoModeSupported)
                {
                    contrast.TrySetAuto(false);
                }

                contrast.TrySetValue(value);
            }
        }

        public void AdjustBrightness(double value)
        {
            var bright = _mediaCapture.VideoDeviceController.Brightness;

            if (bright.Capabilities.Supported)
            {
                if (bright.Capabilities.AutoModeSupported)
                {
                    bright.TrySetAuto(false);
                }

                if (value > bright.Capabilities.Max)
                    value = bright.Capabilities.Max;

                if (value < bright.Capabilities.Min)
                    value = bright.Capabilities.Min;

                bright.TrySetValue(value);
            }
        }

        public void AdjustExposition(double value)
        {
            var expo = _mediaCapture.VideoDeviceController.Exposure;
            if (expo.Capabilities.Supported)
            {
                if (expo.Capabilities.AutoModeSupported)
                {
                    expo.TrySetAuto(false);
                }

                expo.TrySetValue(value);
            }
        }

        public void AdjustWhite(double value)
        {
            var white = _mediaCapture.VideoDeviceController.WhiteBalance;

            if (white.Capabilities.Supported)
            {
                if (white.Capabilities.AutoModeSupported)
                {
                    white.TrySetAuto(false);
                }

                if (value > white.Capabilities.Max)
                    value = white.Capabilities.Max;

                if (value < white.Capabilities.Min)
                    value = white.Capabilities.Min;

                white.TrySetValue(value);
            }
        }

        #region Orientation
        private void RegisterOrientationEventHandlers()
        {
            // If there is an orientation sensor present on the device, register for notifications
            if (_orientationSensor != null)
            {
                _orientationSensor.OrientationChanged += OrientationSensor_OrientationChanged;
                _deviceOrientation = _orientationSensor.GetCurrentOrientation();
            }

            _displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            _displayOrientation = _displayInformation.CurrentOrientation;
        }

        private void UnregisterOrientationEventHandlers()
        {
            if (_orientationSensor != null)
            {
                _orientationSensor.OrientationChanged -= OrientationSensor_OrientationChanged;
            }

            _displayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
        }

        private void OrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                _deviceOrientation = args.Orientation;
            }
        }

        private async void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            _displayOrientation = sender.CurrentOrientation;

            if (_isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }
        #endregion

        /// <summary>
        /// Starts the preview and adjusts it for for rotation and mirroring after making a request to keep the screen on
        /// </summary>
        /// <returns></returns>
        private async Task StartPreviewAsync()
        {
            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _mediaCapture;
            PreviewControl.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start the preview
            try
            {
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when starting the preview: {0}", ex.ToString());
            }

            // Initialize the preview to the current orientation
            if (_isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }

        private async Task SetPreviewRotationAsync()
        {
            // Populate orientation variables with the current state
            _displayOrientation = _displayInformation.CurrentOrientation;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = RotationHelper.ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (_mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1"), rotationDegrees);
            await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            await CleanupCameraAsync();
        }

        public void Cleanup()
        {
            // Use the dispatcher because this method is sometimes called from non-UI threads
            Task.Run(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await CleanupCameraAsync();
            }));
        }

        /// <summary>
        /// Cleans up the camera resources (after stopping any video recording and/or preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");

            if (_isInitialized)
            {
                if (_isPreviewing)
                {
                    // The call to stop the preview is included here for completeness, but can be
                    // safely removed if a call to MediaCapture.Dispose() is being made later,
                    // as the preview will be automatically stopped at that point
                    await StopPreviewAsync();

                    UnregisterOrientationEventHandlers();
                }

                _isInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Failed -= MediaCapture_Failed;
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        /// <summary>
        /// Stops the preview and deactivates a display request, to allow the screen to go into power saving modes
        /// </summary>
        /// <returns></returns>
        private async Task StopPreviewAsync()
        {
            // Stop the preview
            try
            {
                _isPreviewing = false;
                await _mediaCapture.StopPreviewAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when stopping the preview: {0}", ex.ToString());
            }

            // Cleanup the UI
            PreviewControl.Source = null;
        }

        /// <summary>
        /// Takes a photo to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        public async Task<string> TakePhotoAsync()
        {
            var stream = new InMemoryRandomAccessStream();

            try
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                var photoOrientation = RotationHelper.ConvertOrientationToPhotoOrientation(RotationHelper.GetCameraOrientation(_displayInformation, _deviceOrientation));

                return await ReencodeAndSavePhotoAsync(stream, photoOrientation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when taking a photo: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Applies the given orientation to a photo stream and saves it as a StorageFile
        /// </summary>
        /// <param name="stream">The photo stream</param>
        /// <param name="photoOrientation">The orientation metadata to apply to the photo</param>
        /// <returns></returns>
        private async Task<string> ReencodeAndSavePhotoAsync(IRandomAccessStream stream, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                var file = await Package.Current.InstalledLocation.CreateFileAsync("Miriot.jpeg", CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }

                return file.Path;
            }
        }

        public async Task<object> GetLatestFrame()
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("Can't get frame: camera not yet initialized.");
                return null;
            }

            if (!_isPreviewing)
            {
                Debug.WriteLine("Can't get frame: camera not yet previewing.");
                return null;
            }

            try
            {
                if (_lowLightSupported)
                {
                    var res = await _advancedCapture.CaptureAsync();
                    return res.Frame.SoftwareBitmap;
                }
                else
                {
                    // Get information about the preview
                    var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    // Create a video frame in the desired format for the preview frame
                    VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Nv12, (int)previewProperties.Width, (int)previewProperties.Height);

                    var currentFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame);

                    // Collect the resulting frame
                    SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap;

                    return previewFrame;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<byte[]> GetEncodedBytesAsync(object frame)
        {
            SoftwareBitmap bitmapBgra8 = SoftwareBitmap.Convert(((SoftwareBitmap)frame), BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            byte[] array = null;

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            using (var ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                encoder.SetSoftwareBitmap(bitmapBgra8);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) { return new byte[0]; }

                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }

        public void PersistSettings()
        {
            _mediaCapture.VideoDeviceController.Contrast.TryGetValue(out double contrast);
            _mediaCapture.VideoDeviceController.Brightness.TryGetValue(out double brightness);
            _mediaCapture.VideoDeviceController.Exposure.TryGetValue(out double exposure);
            ApplicationData.Current.LocalSettings.Values["CameraContrast"] = contrast;
            ApplicationData.Current.LocalSettings.Values["CameraBrightness"] = brightness;
            ApplicationData.Current.LocalSettings.Values["CameraExposure"] = exposure;
        }

        #endregion Methods
    }
}