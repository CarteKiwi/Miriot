﻿using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class ReminderModel : WidgetModel
    {
        public override bool IsBuiltIn => true;

        public override WidgetType Type => WidgetType.Reminder;

        public ReminderModel(Widget widgetEntity) : base(widgetEntity)
        {
        }
    }
}
