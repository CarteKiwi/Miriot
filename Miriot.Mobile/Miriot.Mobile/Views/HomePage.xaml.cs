using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomePage
	{
        public HomePage ()
		{
			InitializeComponent ();
        }

        protected override void OnAppearing()
        {
            Badge.RotateTo(980, 10000);
            BadgeLoading.RotateTo(980, 10000);

            base.OnAppearing();
        }

        public void RemoteSystemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ViewModel.SelectCommand.Execute((RomeRemoteSystem)e.SelectedItem);
        }
    }
}