using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class Widget
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public WidgetType Type { get; set; }
        public List<string> Infos { get; set; }
    }
}
