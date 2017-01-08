using Windows.UI.Xaml.Controls;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Controls
{
    public sealed partial class WidgetSport
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
