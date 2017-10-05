using Miriot.Common.Model;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class WeatherModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Weather;

        private string _location;

        public string Location
        {
            get { return _location; }
            set { Set(ref _location, value); }
        }

        public WeatherModel(Widget widget) : base(widget)
        {
            Title = "Météo";
        }

        public override WidgetInfo GetInfos()
        {
            return new WeatherWidgetInfo { Location = Location };
        }

        public override Task LoadInfos()
        {
            var info = _infos.FirstOrDefault();
            if (info == null) return Task.FromResult(0);

            Location = JsonConvert.DeserializeObject<WeatherWidgetInfo>(info).Location;

            return Task.FromResult(0);
        }
    }
}
