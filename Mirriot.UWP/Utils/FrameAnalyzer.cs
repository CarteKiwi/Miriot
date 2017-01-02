using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.System.Threading;
using Windows.UI.Core;
using Miriot.Controls;

namespace Miriot.Utils
{
    public class FrameAnalyzer<TAnalysisResultType>
    {
        private FaceTracker _faceTracker;
        private ThreadPoolTimer _frameProcessingTimer;
        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        private CameraControl _camera;
        public Func<VideoFrame, Task<TAnalysisResultType>> AnalysisFunction { get; set; } = null;

        public async Task AttachAsync(CameraControl camera)
        {
            _camera = camera;
            _faceTracker = await FaceTracker.CreateAsync();
            TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); // 15 fps
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

                if (detectedFaces.Count > 0)
                {
                    
                }
                var previewFrameSize = new Size(currentFrame.SoftwareBitmap.PixelWidth, currentFrame.SoftwareBitmap.PixelHeight);
                //var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                //    this.SetupVisualization(previewFrameSize, detectedFaces);
                //});
            }
            catch (Exception e)
            {
                // Face tracking failed
            }
            finally
            {
                _frameProcessingSemaphore.Release();
            }

            currentFrame.Dispose();
        }

    }
}
