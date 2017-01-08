using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Miriot.Common.Model;

namespace Miriot.Core.Messages
{
    public class LoadWidgetsMessage : MessageBase
    {
        public readonly List<Widget> Widgets;

        public LoadWidgetsMessage(List<Widget> widgets)
        {
            Widgets = widgets;
        }
    }
}
