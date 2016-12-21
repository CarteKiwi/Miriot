namespace Miriot.Common.Model
{
    public class SportWidgetInfo : WidgetInfo
    {
        public string Competition { get; set; }
        public string Sport { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
    }
}
