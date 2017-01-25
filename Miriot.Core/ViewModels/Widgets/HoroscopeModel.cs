using System;
using System.Collections.Generic;
using Miriot.Common.Model;
using System.Linq;
using Miriot.Common.Model.Widgets.Horoscope;
using Newtonsoft.Json;

namespace Miriot.Core.ViewModels.Widgets
{
    public class HoroscopeModel : WidgetModel
    {
        private Signs _sign;

        public Signs Sign
        {
            get { return _sign; }
            set { Set(() => Sign, ref _sign, value); }
        }

        public List<Signs> Signs { get; set; }

        public HoroscopeModel()
        {
            Title = "Horoscope";
            Signs = new List<Signs>(Enum.GetValues(typeof(Signs)).Cast<Signs>().ToList());
        }

        public override WidgetInfo GetInfos()
        {
            return new HoroscopeWidgetInfo { SignId = (int)Sign };
        }

        public override void LoadInfos(List<string> infos)
        {
            var info = infos.FirstOrDefault();
            if (info == null) return;

            Sign = (Signs)JsonConvert.DeserializeObject<HoroscopeWidgetInfo>(info).SignId;
            base.LoadInfos(infos);
        }
    }
}
