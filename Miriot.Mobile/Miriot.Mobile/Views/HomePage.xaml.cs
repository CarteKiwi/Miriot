using GalaSoft.MvvmLight.Ioc;
using Miriot.Core.ViewModels;
using Miriot.Model;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            RotateElement(Badge, CancellationToken.None);

            base.OnAppearing();
        }

        private async Task RotateElement(VisualElement element, CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await element.RotateTo(360, 2000, Easing.Linear);
                await element.RotateTo(0, 0); // reset to initial position
            }
        }

        public void RemoteSystemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ViewModel.SelectCommand.Execute((RomeRemoteSystem)e.SelectedItem);
        }
    }
}