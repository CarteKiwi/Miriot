using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;

namespace Miriot.Services
{
    public class SpeechService
    {
        private readonly SpeechSynthesizer _speechSynthesizer;

        public SpeechService()
        {
            _speechSynthesizer = new SpeechSynthesizer
            {
                Voice = (from voiceInformation in SpeechSynthesizer.AllVoices
                         select voiceInformation).First(e => e.Language == "fr-FR")
            };
        }

        public async Task<Stream> GetStream(string text)
        {
            var stream = await new SpeechSynthesizer().SynthesizeTextToStreamAsync(text);
            return stream.AsStream();
        }
    }
}
