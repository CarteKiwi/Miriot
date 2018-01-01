using System;

namespace Miriot.Common.Model
{
    public class Widget
    {
        public Widget() { }

        public Guid Id { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public WidgetType Type { get; set; }
        public string Infos { get; set; }
        public int MiriotConfigurationId { get; set; }
    }
}
