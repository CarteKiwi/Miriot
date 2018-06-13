using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Miriot.Mobile.Views;
using Miriot.Services;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Miriot.Mobile.Services
{
	public class OAuthService : IOAuthService
	{
		private TaskCompletionSource<string> _tcs;
		PopupLoginView popup;

		public OAuthService()
		{
			popup = new PopupLoginView(new Uri($"https://connect.deezer.com/oauth/auth.php?app_id=178262&redirect_uri=http://google.fr/deezer&perms=basic_access,email"));
			popup.Disappearing += Popup_Disappearing;
			popup.Navigated = OnNavigated;
		}

		public bool IsInitialized { get; set; }

		public void Initialize()
		{
		}

		/// <summary>
		/// Authentication. Client side.
		/// </summary>
		/// <returns>Code</returns>
		public async Task<string> AuthenticationAsync()
		{
			_tcs = new TaskCompletionSource<string>();

			await PopupNavigation.Instance.PushAsync(popup, true);

			return await _tcs.Task;
		}

		private void Popup_Disappearing(object sender, EventArgs e)
		{
			try
			{
				_tcs.SetResult("");
			}
			catch
			{

			}
		}

		private void OnNavigated(WebNavigatedEventArgs e)
		{
			if (e.Url.Contains("deezer?code="))
			{
				_tcs.SetResult(e.Url.Split('=').Last());
			}
		}

		/// <summary>
		/// Gets the user async. Server side.
		/// </summary>
		/// <returns>the deezer user</returns>
		/// <param name="token">Token</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public async Task<string> GetUserAsync(string token)
		{
			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://api.deezer.com/user/");
				var res = await client.GetAsync($"me?access_token={token}");
				var content = await res.Content.ReadAsStringAsync();

				return content;
			}
		}

		/// <summary>
		/// Finalizes the authentication. Server side.
		/// </summary>
		/// <returns>Token</returns>
		/// <param name="code">Code</param>
		public async Task<string> FinalizeAuthenticationAsync(string code)
		{
			var configService = SimpleIoc.Default.GetInstance<IConfigurationService>();
			var keys = await configService.GetKeysByProviderAsync("deezer");
			var appId = keys["appId"];
			var redirectUri = keys["redirectUri"];

			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://connect.deezer.com/oauth/");
				var res = await client.GetAsync($"access_token.php?app_id={appId}&secret={redirectUri}&code{code}");
				var content = await res.Content.ReadAsStringAsync();
				//var response = JsonConvert.DeserializeObject<DeezerResponse>(content);

				return content;
			}
		}
	}
}
