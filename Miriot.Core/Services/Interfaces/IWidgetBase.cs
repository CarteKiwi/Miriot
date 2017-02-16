using Miriot.Common.Model;

namespace Miriot.Core.Services.Interfaces
{
    public interface IWidgetBase
    {
        void SetPosition(int x, int y);
        Widget OriginalWidget { get; set; }
    }
}
