﻿using System;
using Windows.UI.Xaml;

namespace Miriot.Controls
{
    public sealed partial class WidgetTime
    {
        public WidgetTime()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = new TimeSpan(1000) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            TimeTb.Text = DateTime.Now.ToString("hh : mm");
            DateTb.Text = $"{DateTime.Now.ToString("ddd. dd MMM")}";
        }
    }
}