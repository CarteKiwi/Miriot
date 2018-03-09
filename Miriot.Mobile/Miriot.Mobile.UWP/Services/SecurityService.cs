using Miriot.Common.Model;
using Miriot.Services.Interfaces;
using System.Linq;
using Windows.Security.Credentials;

namespace Miriot.Win10.Services
{
    public class SecurityService : ISecurityService
    {
        public OAuthWidgetInfo GetSecureData(string tokenKey)
        {
            var vault = new PasswordVault();
            var passwordCredentials = vault.RetrieveAll();
            var temp = passwordCredentials.FirstOrDefault(c => c.Resource == tokenKey);
            var cred = vault.Retrieve(temp.Resource, temp.UserName);

            return new OAuthWidgetInfo { Token = cred.UserName, TokenSecret = cred.Password, Username = temp.UserName };

        }
    }
}
