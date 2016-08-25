using System.Security.Claims;

namespace PopForums.ExternalLogin
{
	public class ExternalLoginInfo
	{
		public ExternalLoginInfo(ClaimsPrincipal externalPrincipal, string loginProvider, string providerKey,
			string displayName)
		{
			ExternalPrincipal = externalPrincipal;
			LoginProvider = loginProvider;
			ProviderKey = providerKey;
			ProviderDisplayName = displayName;
		}

		public ClaimsPrincipal ExternalPrincipal { get; set; }
		public string LoginProvider { get; set; }
		public string ProviderKey { get; set; }
		public string ProviderDisplayName { get; set; }
	}
}
