using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class TimeModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Time;

        public TimeModel(Widget widgetEntity) : base(widgetEntity)
        {
        }
    }
}
