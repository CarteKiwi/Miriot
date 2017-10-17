using System.IO;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Miriot.Core.Services.Interfaces
{
    public interface ISpeechService
    {
        Task InitializeAsync();
        void Stop();
        Task StartListeningAsync();
        void SetCommand(RelayCommand<string> proceedSpeechCommand);
        bool IsLimited { get; set; }
        Task<IRandomAccessStream> SynthesizeTextToStreamAsync(string text);
    }
}
