using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public class DeezerModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Deezer;

        public DeezerModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

    }
}
