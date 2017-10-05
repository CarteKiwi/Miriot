using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class SportModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Sport;

        public SportModel(Widget widgetEntity) : base(widgetEntity)
        {

        }
    }
}
