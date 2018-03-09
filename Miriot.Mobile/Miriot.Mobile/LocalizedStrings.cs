using Miriot.Resources;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile
{
    [ContentProperty("Key")]
    public class LocalizedStrings : IMarkupExtension
    {
        public string Key { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Strings.ResourceManager.GetString(Key);
        }
    }
}
