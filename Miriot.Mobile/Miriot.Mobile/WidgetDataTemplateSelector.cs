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
            if (item is WeatherModel)
                return WeatherTemplate;
            else if (item is HoroscopeModel)
                return HoroscopeTemplate;
            //else if (item is TwitterModel)
            //    xmlTemplatePath = "TwitterTemplate.xml";
            else if (item is CalendarModel)
                return CalendarTemplate;
            else
                return DefaultTemplate;
        }
    }
}