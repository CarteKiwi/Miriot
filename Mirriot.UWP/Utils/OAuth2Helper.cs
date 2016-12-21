using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Utils
{
    public class OAuth2Helper
    {
        private const string FitbitWebAuthBaseUrl = "https://www.fitbit.com";
        private const string FitbitApiBaseUrl = "https://api.fitbit.com";

        private const string OAuthBase = "/oauth2";

        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RedirectUri;

        public OAuth2Helper(string clientId, string secret, string redirectUri)
        {
            this.ClientId = clientId;
            this.ClientSecret = secret;
            this.RedirectUri = redirectUri;
        }
        public string GenerateAuthUrl(string[] scopeTypes, string state = null)
        {
            var sb = new StringBuilder();

            sb.Append(FitbitWebAuthBaseUrl);
            sb.Append(OAuthBase);
            sb.Append("/authorize?");
            sb.Append("response_type=code");
            sb.Append(string.Format("&client_id={0}", this.ClientId));
            sb.Append(string.Format("&redirect_uri={0}", Uri.EscapeDataString(this.RedirectUri)));
            sb.Append(string.Format("&scope={0}", String.Join(" ", scopeTypes)));

            if (!string.IsNullOrWhiteSpace(state))
                sb.Append(string.Format("&state={0}", state));

            return sb.ToString();
        }

        public async Task<OAuth2AccessToken> ExchangeAuthCodeForAccessTokenAsync(string code)
        {
            HttpClient httpClient = new HttpClient();

            string postUrl = OAuth2Helper.FitbitOauthPostUrl;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", ClientId),
                //new KeyValuePair<string, string>("client_secret", AppSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", this.RedirectUri)
            });

            string clientIdConcatSecret = OAuth2Helper.Base64Encode(ClientId + ":" + ClientSecret);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientIdConcatSecret);

            HttpResponseMessage response = await httpClient.PostAsync(postUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            OAuth2AccessToken accessToken = OAuth2Helper.ParseAccessTokenResponse(responseString);

            return accessToken;
        }

        public static readonly string FitbitOauthPostUrl = "https://api.fitbit.com/oauth2/token";


        public static OAuth2AccessToken ParseAccessTokenResponse(string responseString)
        {
            // assumption is the errors json will return in usual format eg. errors array
            JObject responseObject = JObject.Parse(responseString);

            var error = responseObject["errors"];
            if (error != null)
            {
                //var errors = new JsonDotNetSerializer().ParseErrors(responseString);
                //throw new FitbitTokenException(errors);
            }

            return JsonConvert.DeserializeObject<OAuth2AccessToken>(responseString);
        }

        /// <summary>
        /// Convert plain text to a base 64 encoded string - http://stackoverflow.com/a/11743162
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public async Task<OAuth2AccessToken> RefreshToken(string refreshToken)
        {
            string postUrl = FitbitOauthPostUrl;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            });

            var httpClient = new HttpClient();

            var clientIdConcatSecret = OAuth2Helper.Base64Encode(ClientId + ":" + ClientSecret);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientIdConcatSecret);

            HttpResponseMessage response = await httpClient.PostAsync(postUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            return OAuth2Helper.ParseAccessTokenResponse(responseString);
        }
    }
}
