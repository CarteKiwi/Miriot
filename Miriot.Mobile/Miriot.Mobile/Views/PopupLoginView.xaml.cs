using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Miriot.Mobile.Views
{
	public partial class PopupLoginView : PopupPage
	{
		public Action<WebNavigatedEventArgs> Navigated { get; set; }

		public PopupLoginView()
		{
			InitializeComponent();
		}

		public PopupLoginView(Uri uri)
		{
			InitializeComponent();
			Browser.Navigated += handler;
			Browser.Source = uri;
		}

		void handler(object sender, WebNavigatedEventArgs e)
		{
			Navigated?.Invoke(e);
			Loader.IsRunning = false; Loader.IsVisible = false;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
		}

		private async void OnClose(object sender, EventArgs e)
		{
			//Browser.Navigated -= handler;
			await PopupNavigation.Instance.PopAsync();
		}
	}
}
