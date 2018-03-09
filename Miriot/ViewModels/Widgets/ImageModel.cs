using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class ImageModel : WidgetModel
    {
        public override bool IsBuiltIn => true;

        public override WidgetType Type => WidgetType.Image;

        public ImageModel(Widget widgetEntity) : base(widgetEntity)
        {
        }
    }
}
