using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class SncfModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Sncf;

        public SncfModel(Widget widgetEntity) : base(widgetEntity)
        {
        }
    }
}
