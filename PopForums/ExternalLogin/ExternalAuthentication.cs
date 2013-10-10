using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using PopForums.Extensions;

namespace PopForums.ExternalLogin
{
	public class ExternalAuthentication : IExternalAuthentication
	{
		public async Task<ExternalAuthenticationResult> GetAuthenticationResult(IAuthenticationManager authenticationManager)
		{
			var authResult = await authenticationManager.AuthenticateAsync(ExternalCookieName);
			if (authResult == null)
				return null;
			if (!authResult.Identity.IsAuthenticated)
				return null;
			var externalIdentity = authResult.Identity;
			var providerKeyClaim = externalIdentity.FindFirst(ClaimTypes.NameIdentifier);
			var issuer = providerKeyClaim.Issuer;
			var providerKey = providerKeyClaim.Value;
			var name = externalIdentity.FindFirstValue(ClaimTypes.Name);
			var email = externalIdentity.FindFirstValue(ClaimTypes.Email);
			if (String.IsNullOrEmpty(issuer))
				throw new NullReferenceException("The identity claims contain no issuer.");
			if (String.IsNullOrEmpty(providerKey))
				throw new NullReferenceException("The identity claims contain no provider key");
			var result = new ExternalAuthenticationResult
			             {
				             Issuer = issuer,
				             ProviderKey = providerKey,
				             Name = name,
				             Email = email
			             };
			return result;
		}

		public const string ExternalCookieName = "External";
	}
}