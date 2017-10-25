﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.System.Threading;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Win10.Utils
{
    public class FrameAnalyser<T> : IFrameAnalyzer<T>
    {
        private readonly IFileService _fileService;
        public event EventHandler OnPreAnalysis;
        public event EventHandler<T> UsersIdentified;
        public event EventHandler NoFaceDetected;

        private FaceTracker _faceTracker;
        private Timer _frameProcessingTimer;
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
            _faceTracker = await FaceTracker.CreateAsync();
            _frameProcessingTimer = new Timer((a) => { ProcessCurrentVideoFrame(); });
            _frameProcessingTimer.Change(0, 300);
        }

        public async void ProcessCurrentVideoFrame()
        {
            if (!_frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            VideoFrame currentFrame = await GetVideoFrameSafe();

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

                    var output = await AnalysisFunction(currentFrame.SoftwareBitmap.ToByteArray());
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

        private async Task<VideoFrame> GetVideoFrameSafe()
        {
            try
            {
                return (VideoFrame)await _camera.GetLatestFrame();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public void Cleanup()
        {
            _frameProcessingTimer = null;
        }

        public async Task<byte[]> GetFrame()
        {
            var frame = await GetVideoFrameSafe();
            return await _fileService.EncodedBytes(frame?.SoftwareBitmap.ToByteArray());
        }
    }
}
