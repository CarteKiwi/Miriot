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

        private WidgetStates _state;
        public WidgetStates State
        {
            get { return _state; }
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

        public Widget OriginalWidget { get; set; }
    }
}
