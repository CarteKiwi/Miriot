using Miriot.Common.Model;
using Newtonsoft.Json;
using System.Linq;

namespace Miriot.Controls
{
    public sealed partial class WidgetSport
    {
        public WidgetSport(Widget widget) : base(widget)
        {
            InitializeComponent();

            var info = JsonConvert.DeserializeObject<SportWidgetInfo>(widget.Infos.First());

            TitleTb.Text = info.Competition;
            Score1Tb.Text = info.Score1.ToString();
            Score2Tb.Text = info.Score2.ToString();
            Team1Tb.Text = info.Team1;
            Team2Tb.Text = info.Team2;
        }
    }
}
