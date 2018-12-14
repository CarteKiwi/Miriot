using Miriot.Core.ViewModels.Widgets;
using System;
using System.Globalization;
using Windows.UI.Xaml;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetTime
    {
        private bool _secondDisplayed;
        private DispatcherTimer _timer;

        public WidgetTime(TimeModel widget) : base(widget)
        {
            InitializeComponent();

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            Hours.Text = DateTime.Now.ToString("hh", CultureInfo.CurrentUICulture);
            Minutes.Text = DateTime.Now.ToString("mm", CultureInfo.CurrentUICulture);
            DateTb.Text = $"{DateTime.Now.ToString("ddd dd MMM", CultureInfo.CurrentUICulture)}";
            if (_secondDisplayed)
            {
                Seconds.Visibility = Visibility.Visible;
                _secondDisplayed = false;
            }
            else
            {
                Seconds.Visibility = Visibility.Collapsed;
                _secondDisplayed = true;
            }
        }

        public override void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            base.Dispose();
        }
    }
}
