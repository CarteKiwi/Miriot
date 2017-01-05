using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Miriot.Core.Helpers
{
    public class VisionHelper
    {
        private const string OxfordComputerKey = "4c0b719d30004b6b8388b11066b0a7e6";

        public static async void DescribeScene(byte[] bitmap)
        {
            VisionServiceClient client = new VisionServiceClient(OxfordComputerKey);

            using (var stream = new MemoryStream(bitmap))
            {
                var vision = await client.DescribeAsync(stream);

                IsToothBrushing(vision);
            }

        }

        public static bool IsToothBrushing(AnalysisResult result)
        {
            return result.Description.Tags.FirstOrDefault(e => e == "teeth") != null;
        }
    }
}
