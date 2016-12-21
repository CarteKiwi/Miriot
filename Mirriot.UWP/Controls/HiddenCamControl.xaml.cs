using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace Mirriot.Controls
{
    public partial class HiddenCamControl : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isInitialized;

        // Information about the camera device
        private string _cameraId;

        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls       
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();
        private readonly SimpleOrientationSensor _orientationSensor = SimpleOrientationSensor.GetDefault();

        public HiddenCamControl()
        {
            InitializeComponent();
        }

        #region Camera methods

        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, starts preview and unlocks the UI
        /// </summary>
        /// <returns></returns>
        public async Task InitializeCameraAsync()
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

                    DeviceInformation cameraDevice = cameraDevices.First();

                    foreach (var device in cameraDevices)
                    {
                        if (device.EnclosureLocation == null)
                        {
                            _cameraId = device.Id;
                            break;
                        }

                        switch (device.EnclosureLocation.Panel)
                        {
                            case Windows.Devices.Enumeration.Panel.Front:
                                _cameraId = device.Id;
                                //_hasFrontCamera = true;
                                break;
                            case Windows.Devices.Enumeration.Panel.Back:
                                //_rearCameraId = device.Id;
                                break;
                            default:
                                //you can also check for Top, Left, right and Bottom
                                break;
                        }
                    }

                    await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = _cameraId });

                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception when initializing MediaCapture with {0}: {1}", _cameraId, ex.ToString());
                }
            }
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            CleanupCamera();
        }

        /// <summary>
        /// Cleans up the camera resources (after stopping any video recording and/or preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private void CleanupCamera()
        {
            Debug.WriteLine("CleanupCameraAsync");

            if (_isInitialized)
            {
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
        /// Takes a photo to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        public async Task<string> TakePhotoAsync()
        {
            var stream = new InMemoryRandomAccessStream();

            try
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                var photoOrientation = RotationHelper.ConvertOrientationToPhotoOrientation(RotationHelper.GetCameraOrientation(_displayInformation, SimpleOrientation.Faceup));

                return await ReencodeAndSavePhotoAsync(stream, photoOrientation);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when taking a photo: {0}", ex.ToString());
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

                var file = await Package.Current.InstalledLocation.CreateFileAsync("4shot.jpeg", CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }

                SettingsHelper.SaveObjectToStorage("CurrentProfile", new Profile { Name = "Unknown", ImagePath = file.Path });

                return file.Path;
            }
        }
        #endregion Methods
    }
}
