using Windows.UI.Xaml.Controls;

namespace Miriot.Controls
{
    public sealed partial class WidgetSport : WidgetBase
    {
        public WidgetSport(Miriot.Common.Model.SportWidgetInfo info)
        {
            InitializeComponent();

            TitleTb.Text = info.Competition;
            Score1Tb.Text = info.Score1.ToString();
            Score2Tb.Text = info.Score2.ToString();
            Team1Tb.Text = info.Team1;
            Team2Tb.Text = info.Team2;
        }
    }
}
