using System;
using Miriot.Common;
using Miriot.Common.Model;
using Miriot.Win10.Controls;
using Miriot.Core.ViewModels.Widgets;
using System.Collections.Generic;
using Miriot.Core;

namespace Miriot.Win10.Utils
{
    public static class WidgetExtensions
    {
        private static Dictionary<WidgetType, Type> _mapping = new Dictionary<WidgetType, Type>
        {
            { WidgetType.Time, typeof(WidgetTime) },
            { WidgetType.Fitbit, typeof(WidgetFitbit) },
            { WidgetType.Calendar, typeof(WidgetCalendar) },
            { WidgetType.Sncf, typeof(WidgetSncf) },
            { WidgetType.Weather, typeof(WidgetWeather) },
            { WidgetType.Horoscope, typeof(WidgetHoroscope) },
            { WidgetType.Sport, typeof(WidgetSport) },
            { WidgetType.Twitter, typeof(WidgetTwitter) },
            { WidgetType.Deezer, typeof(WidgetDeezer) },
            { WidgetType.Radio, typeof(WidgetRadio) },
            { WidgetType.Reminder, typeof(WidgetReminder) },
            { WidgetType.Image, typeof(WidgetBase) },
        };

        public static WidgetBase ToControl(this WidgetModel widget)
        {
            return (WidgetBase)Activator.CreateInstance(_mapping[widget.Type], widget);
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
