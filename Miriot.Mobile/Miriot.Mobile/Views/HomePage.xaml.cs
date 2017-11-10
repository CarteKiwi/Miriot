using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Core;
using Miriot.Services;
using Miriot.Core.ViewModels;
using Miriot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Miriot.Model;

namespace Miriot.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomePage : ContentPage
	{
        public ConnectViewModel Vm => SimpleIoc.Default.GetInstance<ConnectViewModel>();

        public HomePage ()
		{
			InitializeComponent ();
            BindingContext = Vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Badge.RotateTo(980, 10000);
            BadgeLoading.RotateTo(980, 10000);

            await Vm.InitializeAsync();
        }

        public void RemoteSystemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Vm.SelectCommand.Execute((RomeRemoteSystem)e.SelectedItem);
        }
    }
}