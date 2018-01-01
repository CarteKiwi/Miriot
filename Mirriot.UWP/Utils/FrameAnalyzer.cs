﻿using Miriot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.System.Threading;

namespace Miriot.Win10.Utils
{
    public class FrameAnalyser<T> : IFrameAnalyzer<T>
    {
        private readonly IFileService _fileService;
        public event EventHandler OnPreAnalysis;
        public event EventHandler<T> UsersIdentified;
        public event EventHandler NoFaceDetected;

        private FaceDetector _faceDetector;
        private ThreadPoolTimer _frameProcessingTimer;
        private readonly SemaphoreSlim _frameProcessingSemaphore = new SemaphoreSlim(1);
        private ICameraService _camera;
        private int _detectedFacesInLastFrame;

        public Func<byte[], Task<T>> AnalysisFunction { get; set; }

        public FrameAnalyser(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task AttachAsync(ICameraService camera)
        {
            _camera = camera;
            _faceDetector = await FaceDetector.CreateAsync();

            var timerInterval = TimeSpan.FromMilliseconds(300);

            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(ProcessCurrentVideoFrame, timerInterval);
        }

        public async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (!_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            var latestFrame = await _camera.GetLatestFrame();

            SoftwareBitmap currentFrame = latestFrame as SoftwareBitmap;

            // Use FaceDetector.GetSupportedBitmapPixelFormats and IsBitmapPixelFormatSupported to dynamically
            // determine supported formats
            const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Nv12;

            if (currentFrame == null || currentFrame.BitmapPixelFormat != faceDetectionPixelFormat)
            {
                _frameProcessingSemaphore.Release();
                return;
            }

            try
            {
                IList<DetectedFace> detectedFaces = await _faceDetector.DetectFacesAsync(currentFrame);

                if (detectedFaces.Count == 0)
                {
                    NoFaceDetected?.Invoke(this, null);
                }
                else if (detectedFaces.Count != _detectedFacesInLastFrame)
                {
                    OnPreAnalysis?.Invoke(this, null);

                    var bytes = await _camera.GetEncodedBytesAsync(currentFrame);
                    var output = await AnalysisFunction(bytes);// currentFrame.SoftwareBitmap.ToByteArray());
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
            _frameProcessingTimer = null;
        }

        public async Task<byte[]> GetFrame()
        {
            SoftwareBitmap frame = await _camera.GetLatestFrame() as SoftwareBitmap;
            var bytes = await _camera.GetEncodedBytesAsync(frame);
            frame.Dispose();
            return bytes;
        }
    }
}
