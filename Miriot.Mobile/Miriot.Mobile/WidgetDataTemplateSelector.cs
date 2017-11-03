using Miriot.Core.ViewModels.Widgets;
using System.Xml.Linq;
using Xamarin.Forms;

namespace Miriot.Mobile
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HoroscopeTemplate { get; set; }
        public DataTemplate WeatherTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            string xmlTemplatePath;

            if (item is WeatherModel)
                xmlTemplatePath = "/Templates/WeatherTemplate.xml";
            else if (item is HoroscopeModel)
                xmlTemplatePath = "/Templates/HoroscopeTemplate.xml";
            else if (item is TwitterModel)
                xmlTemplatePath = "/Templates/TwitterTemplate.xml";
            else if (item is CalendarModel)
                xmlTemplatePath = "/Templates/CalendarMailTemplate.xml";
            else
                return DefaultTemplate;

            var templateXml = XDocument.Load(xmlTemplatePath);

            var page = new ContentPage();
            var xaml = page.LoadFromXaml(templateXml.ToString());

            var template = xaml.Resources["DefaultTemplate"] as DataTemplate;
            //var res = Xamarin.Forms.Xaml.Extensions.LoadFromXaml(templateXml.ToString(), typeof(DataTemplate));
            //var template = (DataTemplate)res;


            //var template = (DataTemplate)XamlReader.Load(templateXml.ToString());
            return template;
        }
    }

}
