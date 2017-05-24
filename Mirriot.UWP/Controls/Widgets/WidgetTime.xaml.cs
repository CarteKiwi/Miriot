﻿using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Miriot.Controls
{
    public sealed partial class WidgetTime
    {
        private bool _secondDisplayed;

        public WidgetTime()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            timer.Tick += Timer_Tick;
            timer.Start();
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
    }
}
