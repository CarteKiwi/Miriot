using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.System.Threading;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Utils
{
    public class FrameAnalyser<T> : IFrameAnalyzer<T>
    {
        public event EventHandler OnPreAnalysis;
        public event EventHandler<T> UsersIdentified;
        public event EventHandler NoFaceDetected;

        private FaceTracker _faceTracker;
        private ThreadPoolTimer _frameProcessingTimer;
        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        private ICameraService _camera;
        private int _detectedFacesInLastFrame;

        public Func<SoftwareBitmap, Task<T>> AnalysisFunction { get; set; }

        public async Task AttachAsync(ICameraService camera)
        {
            _camera = camera;
            _faceTracker = await FaceTracker.CreateAsync();
            var timerInterval = TimeSpan.FromMilliseconds(300);
            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(ProcessCurrentVideoFrame, timerInterval);
        }

        public async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
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

                if (detectedFaces.Count == 0)
                {
                    NoFaceDetected?.Invoke(this, null);
                }
                else if (detectedFaces.Count != _detectedFacesInLastFrame)
                {
                    OnPreAnalysis?.Invoke(this, null);

                    var output = await AnalysisFunction(currentFrame.SoftwareBitmap);
                    UsersIdentified?.Invoke(this, output);
                }

                _detectedFacesInLastFrame = detectedFaces.Count;
            }
            catch (Exception ex)
            {
                // Face tracking failed
                Debug.WriteLine(ex);
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
