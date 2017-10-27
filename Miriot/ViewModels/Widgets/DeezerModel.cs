using System.Threading.Tasks;
using Miriot.Common.Model;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using Miriot.Common.Model.Widgets.Deezer;
using System.Linq;
using Miriot.Services;

namespace Miriot.Core.ViewModels.Widgets
{
    public class DeezerModel : WidgetModel
    {
        public override WidgetType Type => WidgetType.Deezer;

        public DeezerModel(Widget widgetEntity) : base(widgetEntity)
        {
        }

        public override async Task LoadInfos()
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
