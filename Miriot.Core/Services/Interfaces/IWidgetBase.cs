using Miriot.Common.Model;

namespace Miriot.Core.Services.Interfaces
{
    public interface IWidgetBase
    {
        WidgetStates State { get; set; }
        void SetPosition(int x, int y);
        Widget OriginalWidget { get; set; }
    }

    public enum WidgetStates
    {
        Iconic,
        Minimal,
        Compact,
        Large
    }
}
