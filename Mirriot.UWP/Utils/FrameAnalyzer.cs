using Miriot.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.System.Threading;

namespace Miriot.Utils
{
    public class FrameAnalyzer<TAnalysisResultType>
    {
        public event EventHandler<TAnalysisResultType> UsersIdentified;
        public event EventHandler NoFaceDetected;

        private FaceTracker _faceTracker;
        private ThreadPoolTimer _frameProcessingTimer;
        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        private CameraControl _camera;
        private int _detectedFacesInLastFrame;

        public Func<VideoFrame, Task<TAnalysisResultType>> AnalysisFunction { private get; set; }

        public async Task AttachAsync(CameraControl camera)
        {
            _camera = camera;
            _faceTracker = await FaceTracker.CreateAsync();
            var timerInterval = TimeSpan.FromMilliseconds(66); // 15 fps
            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(ProcessCurrentVideoFrame, timerInterval);
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (!_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            VideoFrame currentFrame = await _camera.GetLatestFrame();

            // Use FaceDetector.GetSupportedBitmapPixelFormats and IsBitmapPixelFormatSupported to dynamically
            // determine supported formats
            const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Nv12;

            if (currentFrame == null || currentFrame.SoftwareBitmap.BitmapPixelFormat != faceDetectionPixelFormat)
            {
                _frameProcessingSemaphore.Release();
                return;
            }

            try
            {
                IList<DetectedFace> detectedFaces = await _faceTracker.ProcessNextFrameAsync(currentFrame);

                // If number of faces has changed
                if (detectedFaces.Count != _detectedFacesInLastFrame)
                {
                    if (detectedFaces.Count == 0)
                    {
                        NoFaceDetected?.Invoke(this, null);
                    }
                    else
                    {
                        var output = await AnalysisFunction(currentFrame);
                        UsersIdentified?.Invoke(this, output);
                    }
                }

                _detectedFacesInLastFrame = detectedFaces.Count;
            }
            catch (Exception)
            {
                // Face tracking failed
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }

            currentFrame.Dispose();
        }

        public void Cleanup()
        {
            _frameProcessingTimer.Cancel();
            _frameProcessingTimer = null;
        }
    }
}
