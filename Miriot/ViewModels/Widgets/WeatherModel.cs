using Miriot.Common.Model;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class WeatherModel : WidgetModel<WeatherWidgetInfo>
    {
        public override WidgetType Type => WidgetType.Weather;

        public override string Title => "Météo";

        private string _location;

        public string Location
        {
            get { return _location; }
            set { Set(ref _location, value); }
        }

        public WeatherModel(Widget widget) : base(widget)
        {
        }

        public override WeatherWidgetInfo GetModel()
        {
            return new WeatherWidgetInfo { Location = Location };
        }

        public override Task Load()
        {
            base.Load();
            Location = Model?.Location;

            return Task.FromResult(0);
        }
    }
}
