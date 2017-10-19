using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Resources;

namespace Miriot.Win10
{
    public class LocalizedStrings : CustomXamlResourceLoader
    {
        private static readonly ResourceLoader ResourceLoader = new ResourceLoader();

        protected override object GetResource(string resourceId, string objectType, string propertyName, string propertyType)
        {
            return ResourceLoader.GetString(resourceId);
        }
    }
}
