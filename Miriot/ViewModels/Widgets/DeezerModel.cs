using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Model;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
	public class DeezerModel : WidgetModel<DeezerUser>
	{
		public DeezerUser User
		{
			get { return Model; }
			set { Model = value; }
		}

		public override WidgetType Type => WidgetType.Deezer;

		public override string Title => "Musique avec Deezer";

		public DeezerModel(Widget widgetEntity) : base(widgetEntity)
		{
		}

		public override DeezerUser GetModel()
		{
			return User;
		}

		public override void OnActivated()
		{
			if (User == null)
			{
				var dispatcher = SimpleIoc.Default.GetInstance<IDispatcherService>();
				dispatcher.Invoke(async () =>
				{
					var auth = SimpleIoc.Default.GetInstance<IOAuthService>();

					// Display popup login page
					var code = await auth.AuthenticationAsync();

					var remoteService = SimpleIoc.Default.GetInstance<RemoteService>();
					User = await remoteService.GetAsync(RemoteCommands.DeezerService_GetUser, new DeezerUser { Code = code });

					if (User == null)
						IsActive = false;
				});
			}
		}

		public override async void OnDisabled()
		{
			User = null;
		}

		public override async Task Load()
		{
			//var configService = SimpleIoc.Default.GetInstance<IConfigurationService>();
			//var keys = await configService.GetKeysByProviderAsync("deezer");
			//var appId = keys["appId"];
			//var redirectUri = keys["redirectUri"];

			//using (HttpClient client = new HttpClient())
			//{
			//    client.BaseAddress = new Uri("https://connect.deezer.com/oauth/");
			//    var res = await client.GetAsync($"auth.php?app_id={appId}&redirect_uri={redirectUri}&perms=basic_access,email");
			//    var content = await res.Content.ReadAsStringAsync();
			//    var response = JsonConvert.DeserializeObject<DeezerResponse>(content);

			//    var data = response.data;
			//    if (data != null && data.Any())
			//    {
			//        var music = data.First();

			//        // If no params, play random music 
			//        if (string.IsNullOrEmpty(q))
			//        {
			//            var i = _rnd.Next(data.Count() - 1);
			//            music = data[i];
			//        }

			//        await Play(music);
			//    }
			//}
		}
	}
}
