using System;
using System.Threading.Tasks;

namespace Miriot.Services
{
	public interface IOAuthService
	{
		Task<string> AuthenticationAsync();
		Task<string> FinalizeAuthenticationAsync(string code);
		Task<string> GetUserAsync(string token);
	}
}
