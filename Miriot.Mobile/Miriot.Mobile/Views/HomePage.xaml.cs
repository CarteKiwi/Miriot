using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Core;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Standard.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomePage : ContentPage
	{
        public HomePage ()
		{
			InitializeComponent ();
            BindingContext = SimpleIoc.Default.GetInstance<MainViewModel>();


        }

        private void OnStartingIdentification(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnNoFaceDetected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnUsersIdentified(object sender, ServiceResponse e)
        {
            throw new NotImplementedException();
        }
    }
}