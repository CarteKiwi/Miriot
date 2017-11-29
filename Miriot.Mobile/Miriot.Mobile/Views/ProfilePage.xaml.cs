using Miriot.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage
    {
        public ProfilePage(MiriotParameter parameter)
        {
            InitializeComponent();
            
            ViewModel.SetParameters(parameter);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public void RemoteSystemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //ViewModel.SelectCommand.Execute((RomeRemoteSystem)e.SelectedItem);
        }
    }
}