using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Miriot.Win10.Utils.Graph
{
    public class GraphHelper
    {
        const string serviceEndpoint = "https://graph.microsoft.com/v1.0/";
        static string tenant = App.Current.Resources["ida:Domain"].ToString();

        //public static async Task<User> GetUser()
        //{
        //    var user = await new HttpHelper().GetItemAsync<User>(string.Empty);

        //    return user;
        //}

        // Returns information about the signed-in user from Azure Active Directory.

        public static ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        public static async Task<string> GetMeAsync()
        {
            string currentUser = null;
            JObject jResult = null;

            try
            {
                HttpClient client = new HttpClient();
                //var token = await AuthenticationHelper.GetTokenHelperAsync();
                var token = _settings.Values["O365Token"];

                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.ToString());

                // Endpoint for all users in an organization
                Uri usersEndpoint = new Uri(serviceEndpoint + "me");

                HttpResponseMessage response = await client.GetAsync(usersEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    jResult = JObject.Parse(responseContent);
                    currentUser = (string)jResult["displayName"];
                    Debug.WriteLine("Got user: " + currentUser);
                }
                else
                {
                    Debug.WriteLine("We could not get the current user. The request returned this status code: " + response.StatusCode);
                    return null;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("We could not get the current user: " + e.Message);
                return null;
            }

            return currentUser;
        }

    }
}
