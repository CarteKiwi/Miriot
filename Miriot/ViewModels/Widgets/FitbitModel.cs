using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class FitbitModel : WidgetModel
    {
        public FitbitModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

        public override WidgetType Type { get => WidgetType.Fitbit; }
    }
}
