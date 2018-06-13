using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Ioc;
using Miriot.Model;
using Miriot.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Miriot.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CameraPage
	{
		public CameraPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}

		protected override void OnAppearing()
		{
			ViewModel.Initialize();
		}

		void Handle_ValueChanged(object sender, Xamarin.Forms.ValueChangedEventArgs e)
		{
			ViewModel.AdjustCommand.Execute(ViewModel.Brightness);
		}
	}
}
