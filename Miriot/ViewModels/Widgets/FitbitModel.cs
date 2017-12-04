using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class FitbitModel : WidgetModel
    {
        public override string Title => "Composant Fitbit";

        public FitbitModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

        public override WidgetType Type { get => WidgetType.Fitbit; }
    }
}
