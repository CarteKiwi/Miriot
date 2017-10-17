using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.System.Threading;

namespace Miriot.Core.Services.Interfaces
{
    public interface IFrameAnalyzer<T>
    {
        event EventHandler OnPreAnalysis;
        event EventHandler<T> UsersIdentified;
        event EventHandler NoFaceDetected;

        Func<SoftwareBitmap, Task<T>> AnalysisFunction { get; set; }

        Task AttachAsync(ICameraService camera);

        void ProcessCurrentVideoFrame(ThreadPoolTimer timer);

        void Cleanup();

        Task<byte[]> GetFrame();
    }
}
