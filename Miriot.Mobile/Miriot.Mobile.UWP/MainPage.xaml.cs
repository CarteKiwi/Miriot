namespace Miriot.Mobile.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            var app = new Mobile.App();
            LoadApplication(app);
        }
    }
}