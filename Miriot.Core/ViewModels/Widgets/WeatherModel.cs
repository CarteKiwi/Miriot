using System.Collections.Generic;
using Miriot.Common.Model;
using System.Linq;
using Newtonsoft.Json;

namespace Miriot.Core.ViewModels.Widgets
{
    public class WeatherModel : WidgetModel
    {
        private string _location;

        public string Location
        {
            get { return _location; }
            set { Set(() => Location, ref _location, value); }
        }

        public WeatherModel()
        {
            Title = "Météo";
        }

        public override WidgetInfo GetInfos()
        {
            return new WeatherWidgetInfo { Location = Location };
        }

        public override void LoadInfos(List<string> infos)
        {
            var info = infos.FirstOrDefault();
            if (info == null) return;

            Location = JsonConvert.DeserializeObject<WeatherWidgetInfo>(info).Location;
            base.LoadInfos(infos);
        }
    }
}
