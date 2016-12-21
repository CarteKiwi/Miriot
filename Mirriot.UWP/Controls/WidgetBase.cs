using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Miriot.Controls
{
    public class WidgetBase : UserControl, IWidgetBase
    {
        public bool IsFullscreen { get; set; }
        public bool IsExclusive { get; set; }

        public string Token { get; set; }

        public Widget OriginalWidget { get; set; }

        public event EventHandler OnInfosChanged;

        public void RaiseOnChanged()
        {
            OnInfosChanged?.Invoke(this, new EventArgs());
        }

        public void SetAlignment(int x, int y)
        {
            switch (x)
            {
                case 0:
                    HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case 1:
                case 2:
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case 3:
                    HorizontalAlignment = HorizontalAlignment.Right;
                    break;
            }

            switch (y)
            {
                case 0:
                    VerticalAlignment = VerticalAlignment.Top;
                    break;
                case 1:
                    VerticalAlignment = VerticalAlignment.Center;
                    break;
                case 2:
                    VerticalAlignment = VerticalAlignment.Bottom;
                    break;
            }
        }
    }
}
