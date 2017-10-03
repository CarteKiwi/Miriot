using Miriot.Common;
using Miriot.Core.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Miriot.Core.Services
{
    public class LuisService : ILuisService
    {
        private IConfigurationService _configurationService;
        private string _luisKey;
        private bool _isInitialized;

        public LuisService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            var config = await _configurationService.GetKeysAsync();
            _luisKey = config["luis"];

            _isInitialized = true;
        }

        public async Task<LuisResponse> AskLuisAsync(string words)
        {
            await InitializeAsync();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://eastus2.api.cognitive.microsoft.com/luis/v2.0/apps/");
                    var res = await client.GetAsync($"e2fa615c-2c3a-4f2d-a82b-5151223d4cca?subscription-key={_luisKey}&q={words}");
                    var c = await res.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<LuisResponse>(c);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
