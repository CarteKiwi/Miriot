namespace Miriot.Common.Model
{
    public class OAuthWidgetInfo : WidgetInfo
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string Username { get; set; }
        public string Code { get; set; }
    }
}
