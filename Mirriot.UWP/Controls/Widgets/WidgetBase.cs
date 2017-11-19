using Miriot.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Miriot.Win10.Controls
{
    public class WidgetBase : UserControl
    {
        public WidgetBase(int x, int y)
        {
            Margin = new Thickness(20);
            SetPosition(x, y);
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
