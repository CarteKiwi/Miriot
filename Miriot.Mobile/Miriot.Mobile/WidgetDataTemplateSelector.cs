using Miriot.Core.ViewModels.Widgets;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Xamarin.Forms;

namespace Miriot.Mobile
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CalendarTemplate { get; set; }
        public DataTemplate HoroscopeTemplate { get; set; }
        public DataTemplate WeatherTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            string xmlTemplatePath;

            if (item is WeatherModel)
                return WeatherTemplate;
            //else if (item is HoroscopeModel)
            //    xmlTemplatePath = "HoroscopeTemplate.xml";
            //else if (item is TwitterModel)
            //    xmlTemplatePath = "TwitterTemplate.xml";
            else if (item is CalendarModel)
                return CalendarTemplate;
            else
                return DefaultTemplate;

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WidgetTemplateSelector)).Assembly;

            Stream stream = assembly.GetManifestResourceStream($"Miriot.Mobile.Templates.{xmlTemplatePath}");
            string text = "";
            using (var reader = new System.IO.StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            var page = XamlReader.Load<ContentPage>(text);
            var template = page.Resources["DefaultTemplate"] as DataTemplate;

            return template;
        }
    }

}
