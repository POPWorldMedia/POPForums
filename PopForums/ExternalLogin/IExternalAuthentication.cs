using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace PopForums.ExternalLogin
{
	public interface IExternalAuthentication
	{
		Task<ExternalAuthenticationResult> GetAuthenticationResult(IAuthenticationManager authenticationManager);
	}
}