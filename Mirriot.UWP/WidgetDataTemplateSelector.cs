using Miriot.Controls;
using Miriot.Core.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace Miriot
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WeatherTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as Page;

            if (item != null && currentPage != null)
            {
                if (item is WeatherModel)
                {
                    string xmlTemplatePath = Path.Combine(Package.Current.InstalledLocation.Path, "Controls/Widgets/Templates/WeatherTemplate.xml");
                    XDocument templateXml = XDocument.Load(xmlTemplatePath);
                    DataTemplate template = (DataTemplate)XamlReader.Load(templateXml.ToString());

                    return template; //Return the DataTemplateSelector for a disabled tile.
                }
                else
                    return DefaultTemplate; //In case of an error above return the DataTemplateSelector for a normal tile.
            }

            return base.SelectTemplateCore(item, container);
        }
    }

}
