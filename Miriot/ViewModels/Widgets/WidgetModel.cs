using Miriot.Common.Model;

namespace Miriot.Core.ViewModels.Widgets
{
    public abstract class WidgetModel : WidgetModel<object>
    {
        public WidgetModel(Widget widgetEntity) : base(widgetEntity)
        {
            if (widgetEntity == null) return;

            X = widgetEntity.X;
            Y = widgetEntity.Y;
            _infos = widgetEntity.Infos;
        }
    }
}
