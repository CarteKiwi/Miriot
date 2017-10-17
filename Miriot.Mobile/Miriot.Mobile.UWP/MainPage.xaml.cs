namespace Miriot.Mobile.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new Miriot.Mobile.App());
        }
    }
}