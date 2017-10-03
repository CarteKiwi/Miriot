using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;
using System;

namespace Miriot.Core
{
    public static class WidgetExtensions
    {
        public static Type GetModelType(this WidgetType widgetType)
        {
            switch (widgetType)
            {
                case WidgetType.Time:
                    return typeof(TimeModel);
                case WidgetType.Fitbit:
                    return typeof(FitbitModel);
                case WidgetType.Calendar:
                    return typeof(CalendarModel);
                case WidgetType.Sncf:
                    return typeof(SncfModel);
                case WidgetType.Weather:
                    return typeof(WeatherModel);
                case WidgetType.Horoscope:
                    return typeof(HoroscopeModel);
                case WidgetType.Sport:
                    return typeof(SportModel);
                case WidgetType.Twitter:
                    return typeof(TwitterModel);
                case WidgetType.Deezer:
                    return typeof(DeezerModel);
                case WidgetType.Radio:
                    return typeof(RadioModel);
                case WidgetType.Reminder:
                    return typeof(ReminderModel);
                default:
                    return null;
            }
        }
    }
}
