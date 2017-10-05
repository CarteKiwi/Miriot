using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class ImageModel : WidgetModel
    {
        public override WidgetType Type { get => WidgetType.Fitbit; }

        public ImageModel(Widget widgetEntity) : base(widgetEntity)
        {
        }
    }
}
