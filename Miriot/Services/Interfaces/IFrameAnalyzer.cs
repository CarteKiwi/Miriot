using System;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface IFrameAnalyzer<T>
    {
        event EventHandler OnPreAnalysis;
        event EventHandler<T> UsersIdentified;
        event EventHandler NoFaceDetected;

        Func<byte[], Task<T>> AnalysisFunction { get; set; }

        Task AttachAsync(ICameraService camera);

        void Cleanup();

        Task<byte[]> GetFrame();
    }
}
