using GalaSoft.MvvmLight.Command;
using Miriot.Services;
using Miriot.Win10.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;

namespace Miriot.Win10.Services
{
    public class SpeechService : ISpeechService
    {
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechRecognizer _speechRecognizer;
        private SpeechRecognizer _speechRecognizerActivator;
        private RelayCommand<string> _command;

        public bool IsListening { get; set; }

        public void SetCommand(RelayCommand<string> proceedSpeechCommand)
        {
            _command = proceedSpeechCommand;
        }

        private void Cleanup()
        {
            if (_speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                _speechRecognizer.StateChanged -= StateChanged;
                _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;

                _speechRecognizer.Dispose();
                _speechRecognizer = null;
            }

            if (_speechRecognizerActivator != null)
            {
                // cleanup prior to re-initializing this scenario.
                _speechRecognizerActivator.StateChanged -= StateChanged;
                _speechRecognizerActivator.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizerActivator.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;

                _speechRecognizerActivator.Dispose();
                _speechRecognizerActivator = null;
            }
        }

        public async Task InitializeAsync()
        {
            bool granted = await AudioCapturePermissions.RequestMicrophonePermission();

            if (!granted)
            {
                Debug.WriteLine("Access to the microphone denied");
                return;
            }

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

        public async Task StartRecognizerAsync(bool isListening)
        {
            Cleanup();

            if (isListening)
                await InitializeListenerAsync();
            else
                await InitializeActivatorAsync();
        }

        private async Task InitializeActivatorAsync()
        {
            try
            {
                //Language defaultLanguage = SpeechRecognizer.SystemSpeechLanguage;
                //IEnumerable<Language> supportedLanguages = SpeechRecognizer.SupportedTopicLanguages;
                //foreach (Language lang in supportedLanguages)
                //{
                //    Debug.WriteLine(lang.DisplayName + " " + lang.LanguageTag);
                //}

                _speechRecognizerActivator = new SpeechRecognizer(new Language("fr-FR"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpeechRecognizerActivator failed to initialize : check the microphone");
                Debug.WriteLine(ex.Message);
                return;
            }

            var listConstraint = new SpeechRecognitionListConstraint(new[] { "Miriot" });
            _speechRecognizerActivator.Constraints.Add(listConstraint);

            var compileResult = await _speechRecognizerActivator.CompileConstraintsAsync();

            if (compileResult.Status != SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Grammar Compilation Failed: " + compileResult.Status.ToString());
            }

            _speechRecognizerActivator.StateChanged += StateChanged;
            _speechRecognizerActivator.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizerActivator.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            await _speechRecognizerActivator.ContinuousRecognitionSession.StartAsync();
        }

        private async Task InitializeListenerAsync()
        {
            try
            {
                _speechRecognizer = new SpeechRecognizer(new Language("fr-FR"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SpeechRecognizer failed to initialize : check the microphone");
                Debug.WriteLine(ex.Message);
                return;
            }

            _speechRecognizer.Constraints.Add(new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation"));

            var compileResult = await _speechRecognizer.CompileConstraintsAsync();

            if (compileResult.Status != SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Grammar Compilation Failed: " + compileResult.Status.ToString());
            }

            _speechRecognizer.StateChanged += StateChanged;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private void StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine(args.State);
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Text.Contains("Miriot"))
            {
                IsListening = true;
            }
            else
            {
                IsListening = false;
            }

            Debug.WriteLine("Texte reconnu: " + args.Result.Text);
            _command.Execute(args.Result.Text);
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                //if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                //{
                //Enable continuous listening
                StartListeningAsync(IsListening);
                //}
            }
        }

        public async Task StartListeningAsync(bool isListening)
        {
            try
            {
                if (isListening)
                {
                    StopActivator();
                }
                else
                {
                    Stop();
                }

                await StartRecognizerAsync(isListening);
            }
            catch (Exception ex)
            {
                await StartRecognizerAsync(isListening);
                Debug.WriteLine(ex.Message);
            }
        }

        public async void StopActivator()
        {
            if (_speechRecognizerActivator?.State != SpeechRecognizerState.Idle)
            {
                try
                {
                    var asyncAction = _speechRecognizerActivator?.ContinuousRecognitionSession?.StopAsync();
                    if (asyncAction != null)
                        await asyncAction;
                }
                catch (Exception)
                {
                    // Do nothing
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
                _speechSynthesizer.Options.SpeakingRate = 1.2;
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
