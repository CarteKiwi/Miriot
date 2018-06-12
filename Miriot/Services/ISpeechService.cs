using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public interface ISpeechService
    {
        Task InitializeAsync();
        void Stop();
        Task StartListeningAsync(bool isListening);
        void SetCommand(RelayCommand<string> proceedSpeechCommand);
        bool IsListening { get; set; }
        Task<object> SynthesizeTextToStreamAsync(string text);
    }
}
