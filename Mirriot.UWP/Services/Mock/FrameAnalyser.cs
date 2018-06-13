using Miriot.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Miriot.Win10.Services.Mock
{
    public class FrameAnalyser<T> : IFrameAnalyzer<T>
    {
        public event EventHandler OnPreAnalysis;
        public event EventHandler<T> UsersIdentified;
        public event EventHandler NoFaceDetected;

        public Func<byte[], Task<T>> AnalysisFunction { get; set; }

        public Task AttachAsync(ICameraService camera)
        {
            ProcessCurrentVideoFrame(null);
            return Task.FromResult(true);
        }

        public async void ProcessCurrentVideoFrame(Timer timer)
        {
            OnPreAnalysis?.Invoke(this, null);

            var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var uri = p.Path + "/carlo.jpg";

            var array = File.ReadAllBytes(uri);
            
            var output = await AnalysisFunction(array);
            UsersIdentified?.Invoke(this, output);
        }

        public void Cleanup()
        {
        }

        public async Task<byte[]> GetFrame()
        {
            var p = await Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            var uri = p.Path + "/carlo.jpg";

            var array = File.ReadAllBytes(uri);

            return array;
        }
    }
}
