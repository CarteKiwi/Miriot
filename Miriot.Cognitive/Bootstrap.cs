using GalaSoft.MvvmLight.Ioc;
using Miriot.Services;

namespace Miriot.Cognitive
{
    public static class Bootstrap
    {
        public static void Load()
        {
            SimpleIoc.Default.Register<ILuisService, LuisService>();
            SimpleIoc.Default.Register<IFaceService, FaceService>();
            SimpleIoc.Default.Register<IVisionService, VisionService>();
        }
    }
}
