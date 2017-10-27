using GalaSoft.MvvmLight.Command;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;

namespace Miriot.Win10.Services
{
    public class SpeechService : ISpeechService
    {
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechRecognizer _speechRecognizer;
        private RelayCommand<string> _command;

        public bool IsLimited
        {
            get => _speechRecognizer.Constraints.First().IsEnabled;
            set => _speechRecognizer.Constraints.First().IsEnabled = !value;
        }

        public void SetCommand(RelayCommand<string> proceedSpeechCommand)
        {
            _command = proceedSpeechCommand;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _speechRecognizer = new SpeechRecognizer(new Windows.Globalization.Language("fr-FR"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpeechRecognizer failed to initialize : check the microphone");
                Debug.WriteLine(ex.Message);
                return;
            }

            // Add a list constraint to the recognizer.

            // Compile the dictation topic constraint, which optimizes for dictated speech.
            var listConstraint = new SpeechRecognitionListConstraint(new[] { "Miriot" });
            //_speechRecognizer.Constraints.Add(listConstraint);
            _speechRecognizer.Constraints.Add(new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, ""));

            await _speechRecognizer.CompileConstraintsAsync();

            // Stop listening events
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;

            // Start listening events
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            try
            {
                _speechSynthesizer = new SpeechSynthesizer
                {
                    Voice = (from voiceInformation in SpeechSynthesizer.AllVoices
                             select voiceInformation).First(e => e.Language == "fr-FR")
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            _command.Execute(args.Result.Text);
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    //Enable continuous listening
                    StartListeningAsync();
                }
            }
        }

        public async Task StartListeningAsync()
        {
            if (_speechRecognizer?.State == SpeechRecognizerState.Idle)
            {
                try
                {
                    await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception ex)
                {
                    await InitializeAsync();
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async void Stop()
        {
            if (_speechRecognizer?.State != SpeechRecognizerState.Idle)
            {
                try
                {
                    var asyncAction = _speechRecognizer?.ContinuousRecognitionSession?.StopAsync();
                    if (asyncAction != null)
                        await asyncAction;
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        /// <summary>
        /// Make the voice speak the text
        /// When the voice has ended, start listening
        /// </summary>
        /// <param name="text">Text to be spoken</param>
        /// <returns>nothing</returns>
        public async Task<object> SynthesizeTextToStreamAsync(string text)
        {
            if (_speechSynthesizer == null) return null;

            try
            {
                return await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
