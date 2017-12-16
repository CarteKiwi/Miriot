using Miriot.Common.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class MiriotService
    {
        public static string RestUrl = "http://localhost:50388/api/users/{0}";
        //public static string RestUrl = "http://miriot.azurewebsites.net/api/users/{0}";
        private HttpClient _client;

        public MiriotService()
        {
            _client = new HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<bool> CreateUser(User user)
        {
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await _client.PostAsync(uri, content);

            return false;
        }

        public async Task<User> GetUser(Guid id)
        {
            var uri = new Uri(string.Format(RestUrl, id));
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User>(content);
            }

            return null;
        }

        public async Task UpdateUserAsync(User user)
        {
            var uri = new Uri(string.Format(RestUrl, user.Id));

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await _client.PutAsync(uri, content);
        }
    }
}
