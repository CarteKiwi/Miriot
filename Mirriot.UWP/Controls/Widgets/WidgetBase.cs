using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Controls
{
    public class WidgetBase : UserControl, IWidgetBase
    {
        public WidgetBase()
        {
            Margin = new Thickness(20);
        }

        public virtual void SetPosition(int x, int y)
        {
            Grid.SetColumn(this, x);
            Grid.SetRow(this, y);
        }

        public Widget OriginalWidget { get; set; }
    }
}
