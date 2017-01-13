using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace Miriot.Controls
{
    public sealed partial class WidgetTimer
    {
        private Stopwatch _stopWatch;

        public TimeSpan Duration { get; set; }

        public WidgetTimer()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = new TimeSpan(1000) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            var ts = _stopWatch.Elapsed;
            TimerTb.Text = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
        }

        public void Start()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void Stop()
        {
            _stopWatch.Stop();
        }
    }
}
