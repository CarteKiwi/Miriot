using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using System;
using System.Collections.Generic;

namespace Miriot.Core
{
    public static class WidgetMapping
    {
        private static Dictionary<WidgetType, Type> _mapping = new Dictionary<WidgetType, Type>
        {
            { WidgetType.Time, typeof(TimeModel) },
            { WidgetType.Fitbit, typeof(FitbitModel) },
            { WidgetType.Calendar, typeof(CalendarModel) },
            { WidgetType.Sncf, typeof(SncfModel) },
            { WidgetType.Weather, typeof(WeatherModel) },
            { WidgetType.Horoscope, typeof(HoroscopeModel) },
            { WidgetType.Sport, typeof(SportModel) },
            { WidgetType.Twitter, typeof(TwitterModel) },
            { WidgetType.Deezer, typeof(DeezerModel) },
            { WidgetType.Radio, typeof(RadioModel) },
            { WidgetType.Reminder, typeof(ReminderModel) },
            { WidgetType.Image, typeof(ImageModel) },
            { WidgetType.News, typeof(NewsModel) },
        };

        public static WidgetModel ToModel(this Widget widget)
        {
            return (WidgetModel)Activator.CreateInstance(_mapping[widget.Type], widget);
        }

        public static WidgetModel ToModel(this WidgetType type, Widget widget)
        {
            return (WidgetModel)Activator.CreateInstance(_mapping[type], widget);
        }
    }
}
