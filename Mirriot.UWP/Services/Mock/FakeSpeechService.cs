using GalaSoft.MvvmLight.Command;
using Miriot.Services;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;

namespace Miriot.Win10.Services
{
    public class FakeSpeechService : ISpeechService
    {
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechRecognizer _speechRecognizer;
        private RelayCommand<string> _command;

        public bool IsListening
        {
            get => false;
            set { }
        }

        public void SetCommand(RelayCommand<string> proceedSpeechCommand)
        {
            _command = proceedSpeechCommand;
        }

        public async Task InitializeAsync()
        {
        }

        public async Task StartListeningAsync(bool isListening)
        {
            //_command.Execute("Miriot");
            //_command.Execute("Je m'appelle Guillaume");
        }

        public async void Stop()
        {
        }

        /// <summary>
        /// Make the voice speak the text
        /// When the voice has ended, start listening
        /// </summary>
        /// <param name="text">Text to be spoken</param>
        /// <returns>nothing</returns>
        public async Task<object> SynthesizeTextToStreamAsync(string text)
        {
            return null;
        }
    }
}
