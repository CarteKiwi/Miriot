using Miriot.Common.Model;
using Miriot.Common.Model.Widgets.Horoscope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class HoroscopeModel : WidgetModel<HoroscopeWidgetInfo>
    {
        private Signs? _sign;

        public Signs? Sign
        {
            get { return _sign; }
            set { Set(ref _sign, value); }
        }

        public List<Signs> Signs { get; set; }

        public override WidgetType Type => WidgetType.Horoscope;

        public HoroscopeModel(Widget widget) : base(widget)
        {
            Title = "Horoscope";
            Signs = new List<Signs>(Enum.GetValues(typeof(Signs)).Cast<Signs>().ToList());
        }

        public override HoroscopeWidgetInfo GetModel()
        {
            return new HoroscopeWidgetInfo { SignId = (int?)Sign };
        }

        public override Task Load()
        {
            base.Load();

            if (Model?.SignId != null)
                Sign = (Signs)Model.SignId;

            return Task.FromResult(0);
        }
    }
}
