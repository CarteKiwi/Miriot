using System;
using System.Collections.Generic;
using Miriot.Common.Model;
using System.Linq;
using System.Threading.Tasks;
using Miriot.Common.Model.Widgets.Horoscope;
using Newtonsoft.Json;

namespace Miriot.Core.ViewModels.Widgets
{
    public class HoroscopeModel : WidgetModel
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

        public override WidgetInfo GetInfos()
        {
            return new HoroscopeWidgetInfo { SignId = (int?)Sign };
        }

        public override Task LoadInfos()
        {
            var info = _infos.FirstOrDefault();
            if (info == null) return Task.FromResult(0);

            var infoModel = JsonConvert.DeserializeObject<HoroscopeWidgetInfo>(info);

            if (infoModel?.SignId != null)
                Sign = (Signs)infoModel.SignId;

            return Task.FromResult(0);
        }
    }
}
