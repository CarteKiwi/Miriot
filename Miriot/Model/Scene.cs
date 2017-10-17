using Microsoft.ProjectOxford.Vision.Contract;

namespace Miriot.Common.Model
{
    public class Scene
    {
        public bool IsToothbrushing { get; set; }

        public string[] Tags { get; set; }

        public Face[] Faces { get; set; }
    }
}