using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;

namespace Miriot.Core.Services.Interfaces
{
    public interface ISpeechService
    {
        Task InitializeAsync();
        void Stop();
        Task StartListeningAsync();
        void SetCommand(RelayCommand<string> proceedSpeechCommand);
        bool IsLimited { get; set; }
        Task<object> SynthesizeTextToStreamAsync(string text);
    }
}
