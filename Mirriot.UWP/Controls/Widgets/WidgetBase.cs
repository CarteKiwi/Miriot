using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Controls
{
    public class WidgetBase : UserControl, IWidgetBase
    {
        public Widget OriginalWidget { get; set; }

        public WidgetBase(Widget widget)
        {
            Margin = new Thickness(20);
            OriginalWidget = widget;

            if (widget != null)
                SetPosition(widget.X, widget.Y);
        }

        private WidgetStates _state;
        public WidgetStates State
        {
            get => _state;
            set
            {
                _state = value;
                OnStateChanged();
            }
        }

        public virtual void OnStateChanged() { }

        public virtual void SetPosition(int x, int y)
        {
            Grid.SetColumn(this, x);
            Grid.SetRow(this, y);
        }
    }
}
