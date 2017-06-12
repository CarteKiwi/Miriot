using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Miriot.Common.Model;

namespace Miriot.Controls
{
    public sealed partial class WidgetTimer
    {
        private Stopwatch _stopWatch;

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register("IsRunning", typeof(bool), typeof(WidgetTimer), new PropertyMetadata(false, OnRunningChanged));

        private static void OnRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var w = d as WidgetTimer;

            if (w.IsRunning)
                w.Start();
            else
                w.Stop();
        }

        public WidgetTimer()
        {
            InitializeComponent();
        }

        public WidgetTimer(Widget widget) : base(widget)
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = new TimeSpan(1000) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (_stopWatch == null) return;

            var ts = _stopWatch.Elapsed;
            TimerTb.Text = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }

        private void Start()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        private void Stop()
        {
            _stopWatch.Stop();
        }
    }
}
