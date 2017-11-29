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
        private CancellationTokenSource _cancellationToken;

        public HomePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            _cancellationToken = new CancellationTokenSource();
            RotateElement(Badge, _cancellationToken);

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cancellationToken.Cancel();
        }

        private async Task RotateElement(VisualElement element, CancellationTokenSource cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await element.RotateTo(360, 2000, Easing.Linear);
                await element.RotateTo(0, 0);
            }
        }

        public void RemoteSystemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ViewModel.SelectCommand.Execute((RomeRemoteSystem)e.SelectedItem);
        }
    }
}