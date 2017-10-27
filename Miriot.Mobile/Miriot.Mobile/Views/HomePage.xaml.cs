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

            await Vm.InitializeAsync().ConfigureAwait(false);
        }
    }
}