using Miriot.Common.Model;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class NewsModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.News;

        public override string Title => "Actualité / News";

        public NewsModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

        public override async Task Load()
        {
        }
    }
}
