using Miriot.Core.Services.Interfaces;
using Miriot.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.System.Threading;

namespace Miriot.Services.Mock
{
    public class FrameAnalyser<T> : IFrameAnalyzer<T>
    {
        private ThreadPoolTimer _frameProcessingTimer;
        public event EventHandler OnPreAnalysis;
        public event EventHandler<T> UsersIdentified;
        public event EventHandler NoFaceDetected;

        public Func<SoftwareBitmap, Task<T>> AnalysisFunction { get; set; }

        public Task AttachAsync(ICameraService camera)
        {
            var timerInterval = TimeSpan.FromMilliseconds(10000); // 15 fps
            _frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(ProcessCurrentVideoFrame, timerInterval);
            return Task.FromResult(true);
        }

        public async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            OnPreAnalysis?.Invoke(this, null);

            var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var uri = p.Path + "/untitled.png";

            var array = File.ReadAllBytes(uri);
            
            var output = await AnalysisFunction(await array.ToSoftwareBitmap());
            UsersIdentified?.Invoke(this, output);
        }

        public void Cleanup()
        {
            
        }

        public async Task<byte[]> GetFrame()
        {
            var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var uri = p.Path + "/untitled.png";

            var array = File.ReadAllBytes(uri);

            return array;
        }
    }
}
