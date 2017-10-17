using Miriot.Common.Model;
using Miriot.Core.ViewModels.Widgets;

namespace Miriot.Core.Services.Interfaces
{
    public interface IWidgetBase
    {
        WidgetStates State { get; set; }
        void SetPosition(int x, int y);
        WidgetModel OriginalWidget { get; set; }
    }

    public enum WidgetStates
    {
        Iconic,
        Minimal,
        Compact,
        Large
    }
}
