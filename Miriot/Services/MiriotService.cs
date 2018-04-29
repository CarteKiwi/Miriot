using GalaSoft.MvvmLight.Views;
using Miriot.Common.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class MiriotService
    {
        //public static string RestUrl = "http://192.168.1.91:50388/api/{0}/{1}";
        private static string UserController = "users";
        private static string ConfigurationController = "configurations";
        public static string RestUrl = "http://miriot.azurewebsites.net/api/{0}/{1}";
        private HttpClient _client;
        private readonly IDialogService _dialogService;

        public MiriotService(IDialogService dialogService)
        {
            _dialogService = dialogService;

            _client = new HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<bool> CreateUser(User user)
        {
            var uri = new Uri(string.Format(RestUrl, UserController, string.Empty));

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await _client.PostAsync(uri, content);

            return false;
        }

        public async Task<User> GetUser(Guid id)
        {
            try
            {
                var uri = new Uri(string.Format(RestUrl, UserController, id));
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Impossible de contacter le service Miriot: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var uri = new Uri(string.Format(RestUrl, UserController, user.Id));

                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await _client.PutAsync(uri, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Impossible de contacter le service Miriot: {ex.Message}");
            }

            return false;
        }

        public async Task<MiriotConfiguration> CreateConfiguration(MiriotConfiguration config)
        {
            var uri = new Uri(string.Format(RestUrl, ConfigurationController, string.Empty));

            var json = JsonConvert.SerializeObject(config);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await _client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MiriotConfiguration>(result);
            }

            return null;
        }

        public async Task<bool> UpdateConfigurationAsync(MiriotConfiguration config)
        {
            var uri = new Uri(string.Format(RestUrl, ConfigurationController, config.Id));

            var json = JsonConvert.SerializeObject(config);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await _client.PutAsync(uri, content);

            return response.IsSuccessStatusCode;
        }

    }
}
