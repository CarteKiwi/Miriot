using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class RadioModel : WidgetModel
    {
        public RadioModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

        public override WidgetType Type => WidgetType.Radio;
    }
}
