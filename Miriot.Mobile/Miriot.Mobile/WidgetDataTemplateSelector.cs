﻿using Miriot.Core.ViewModels.Widgets;
using System.IO;
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
                xmlTemplatePath = "WeatherTemplate.xml";
            //else if (item is HoroscopeModel)
            //    xmlTemplatePath = "HoroscopeTemplate.xml";
            //else if (item is TwitterModel)
            //    xmlTemplatePath = "TwitterTemplate.xml";
            //else if (item is CalendarModel)
            //    xmlTemplatePath = "CalendarMailTemplate.xml";
            else
                return DefaultTemplate;

            var path = $"{Directory.GetCurrentDirectory()}\\Controls\\Widgets\\Templates\\";

            var templateXml = XDocument.Load(path + xmlTemplatePath);

            var page = XamlReader.Load<Xamarin.Forms.ContentPage>(templateXml.ToString());
            var template = page.Resources["DefaultTemplate"] as DataTemplate;
            
            return template;
        }
    }

}
