using System;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Controls;
using Miriot.Core.ViewModels.Widgets;

namespace Miriot.Utils
{
    public static class WidgetExtensions
    {
        public static Type GetWidgetType(this WidgetModel widget)
        {
            switch (widget.Type)
            {
                case WidgetType.Time:
                    return typeof(WidgetTime);
                case WidgetType.Fitbit:
                    return typeof(WidgetFitbit);
                case WidgetType.Calendar:
                    return typeof(WidgetCalendar);
                case WidgetType.Sncf:
                    return typeof(WidgetSncf);
                case WidgetType.Weather:
                    return typeof(WidgetWeather);
                case WidgetType.Horoscope:
                    return typeof(WidgetHoroscope);
                case WidgetType.Sport:
                    return typeof(WidgetSport);
                case WidgetType.Twitter:
                    return typeof(WidgetTwitter);
                case WidgetType.Deezer:
                    return typeof(WidgetDeezer);
                case WidgetType.Radio:
                    return typeof(WidgetRadio);
                case WidgetType.Reminder:
                    return typeof(WidgetReminder);
                default:
                    return null;
            }
        }

        public static Type GetIntentType(this IntentResponse intent)
        {
            switch (intent.Intent)
            {
                case "PlaySong":
                    return typeof(WidgetDeezer);
               
                case "TurnOnTv":
                case "FullScreenTv":
                case "ReduceScreenTv":
                    return typeof(WidgetTv);
                case "TurnOnRadio":
                    return typeof(WidgetRadio);
                case "BlancheNeige":
                    //Vm.SpeakCommand.Execute("Si je m'en tiens aux personnes que je connais, je peux affirmer que tu es le plus beau.");
                case "Tweet":
                case "HideAll":
                case "StartScreen":
                case "DisplayWidget":
                case "DisplayMail":
                case "AddReminder":
                case "TakePhoto":
                case "TurnOff":
                default:
                    return null;
                
            }
        }
    }
}
