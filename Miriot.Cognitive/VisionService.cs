using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Miriot.Common.Model;
using Miriot.Services;

namespace Miriot.Cognitive
{
    public class VisionService : IVisionService
    {
        private const string OxfordComputerKey = "4c0b719d30004b6b8388b11066b0a7e6";

        public async Task<Scene> CreateSceneAsync(byte[] bitmap)
        {
            var scene = new Scene();

            var client = new VisionServiceClient(OxfordComputerKey);

            using (var stream = new MemoryStream(bitmap))
            {
                var vision = await client.GetTagsAsync(stream);

                scene.IsToothbrushing = IsToothBrushing(vision);
                //scene.Faces = vision.Faces;
            }

            return scene;
        }

        private bool IsToothBrushing(AnalysisResult result)
        {
            return result.Tags?.Where(tag => tag.Confidence > 0.5).FirstOrDefault(tag => tag.Name == "toothbrush") != null;
        }
    }
}
