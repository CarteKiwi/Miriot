using Miriot.Core.ViewModels.Widgets;
using System.IO;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Miriot
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HoroscopeTemplate { get; set; }
        public DataTemplate WeatherTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as Page;

            if (item != null && currentPage != null)
            {
                string xmlTemplatePath;

                if (item is WeatherModel)
                    xmlTemplatePath = Path.Combine(Package.Current.InstalledLocation.Path, "Controls/Widgets/Templates/WeatherTemplate.xml");
                else if (item is HoroscopeModel)
                    xmlTemplatePath = Path.Combine(Package.Current.InstalledLocation.Path, "Controls/Widgets/Templates/HoroscopeTemplate.xml");
                else if (item is TwitterModel)
                    xmlTemplatePath = Path.Combine(Package.Current.InstalledLocation.Path, "Controls/Widgets/Templates/TwitterTemplate.xml");
                else if (item is CalendarModel)
                    xmlTemplatePath = Path.Combine(Package.Current.InstalledLocation.Path, "Controls/Widgets/Templates/CalendarMailTemplate.xml");
                else
                    return DefaultTemplate;

                var templateXml = XDocument.Load(xmlTemplatePath);
                var template = (DataTemplate)XamlReader.Load(templateXml.ToString());
                return template;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

}
