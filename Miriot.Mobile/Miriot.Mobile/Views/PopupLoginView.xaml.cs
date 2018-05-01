using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Miriot.Mobile.Views
{
    public partial class PopupLoginView : PopupPage
    {
        public PopupLoginView()
        {
            InitializeComponent();
        }

        public PopupLoginView(Uri uri)
        {
            InitializeComponent();
            Browser.Source = uri;
            void handler(object sender, WebNavigatedEventArgs e)
            {
                Browser.Navigated -= handler;
                Loader.IsRunning = false; Loader.IsVisible = false;
            }

            Browser.Navigated += handler;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnClose(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
