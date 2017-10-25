using Miriot.Common.Model;

namespace Miriot.Services.Interfaces
{
    public interface ISecurityService
    {
        OAuthWidgetInfo GetSecureData(string tokenKey);
    }
}
