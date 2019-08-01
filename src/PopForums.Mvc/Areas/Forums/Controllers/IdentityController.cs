using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Services;
using PopIdentity;
using PopIdentity.Providers.Facebook;
using PopIdentity.Providers.Google;
using PopIdentity.Providers.Microsoft;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class IdentityController : Controller
	{
		private readonly ILoginLinkFactory _loginLinkFactory;
		private readonly IStateHashingService _stateHashingService;
		private readonly ISettingsManager _settingsManager;
		private readonly IFacebookCallbackProcessor _facebookCallbackProcessor;
		private readonly IGoogleCallbackProcessor _googleCallbackProcessor;
		private readonly IMicrosoftCallbackProcessor _microsoftCallbackProcessor;
		private readonly IOAuth2JwtCallbackProcessor _oAuth2JwtCallbackProcessor;
		private readonly IExternalUserAssociationManager _externalUserAssociationManager;
		private readonly IUserService _userService;

		public IdentityController(ILoginLinkFactory loginLinkFactory, IStateHashingService stateHashingService, ISettingsManager settingsManager, IFacebookCallbackProcessor facebookCallbackProcessor, IGoogleCallbackProcessor googleCallbackProcessor, IMicrosoftCallbackProcessor microsoftCallbackProcessor, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor, IExternalUserAssociationManager externalUserAssociationManager, IUserService userService)
		{
			_loginLinkFactory = loginLinkFactory;
			_stateHashingService = stateHashingService;
			_settingsManager = settingsManager;
			_facebookCallbackProcessor = facebookCallbackProcessor;
			_googleCallbackProcessor = googleCallbackProcessor;
			_microsoftCallbackProcessor = microsoftCallbackProcessor;
			_oAuth2JwtCallbackProcessor = oAuth2JwtCallbackProcessor;
			_externalUserAssociationManager = externalUserAssociationManager;
			_userService = userService;
		}

		public string Name = "Identity";

		public class GenericResult
		{
			public string ID { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
		}

		[HttpPost]
		public IActionResult ExternalLogin(string provider)
		{
			var state = _stateHashingService.SetCookieAndReturnHash();
			switch (provider.ToLower())
			{
				case "facebook":
					var facebookRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
					var facebookLink = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, state, _settingsManager.Current.FacebookAppID);
					return Redirect(facebookLink);
				case "google":
					var googleRedirect = this.FullUrlHelper(nameof(GoogleCallback), Name);
					var googleLink = _loginLinkFactory.GetLink(ProviderType.Google, googleRedirect, state, _settingsManager.Current.GoogleClientId);
					return Redirect(googleLink);
				case "microsoft":
					var msftRedirect = this.FullUrlHelper(nameof(MicrosoftCallback), Name);
					var msftLink = _loginLinkFactory.GetLink(ProviderType.Microsoft, msftRedirect, state, _settingsManager.Current.MicrosoftClientID);
					return Redirect(msftLink);
				case "oauth2":
					var oauthRedirect = this.FullUrlHelper(nameof(OAuth2Callback), Name);
					var linkGenerator = new OAuth2LoginUrlGenerator();
					var oauthClaims = new List<string>(new[] { "openid", "email" });
					var oauthLink = linkGenerator.GetUrl(_settingsManager.Current.OAuth2LoginUrl, _settingsManager.Current.OAuth2ClientID, oauthRedirect, state, oauthClaims);
					return Redirect(oauthLink);
				default: throw new NotImplementedException($"The external login \"{provider}\" is not configured.");
			}
		}
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			if (remoteError != null)
			{
				// TODO: deal with this
			}
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();

			var user = User;

			// do something with the result

			ViewBag.Referrer = returnUrl;
			return Content("test"); // use the view from the old auth controller
		}

		public static async Task PerformSignInAsync(User user, HttpContext httpContext)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Name)
			};

			var props = new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTime.UtcNow.AddYears(1)
			};

			var id = new ClaimsIdentity(claims, PopForumsAuthorizationDefaults.AuthenticationScheme);
			await httpContext.SignInAsync(PopForumsAuthorizationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);
		}

		public async Task<IActionResult> FacebookCallback()
		{
			var facebookRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
			var result = await _facebookCallbackProcessor.VerifyCallback(facebookRedirect, _settingsManager.Current.FacebookAppID, _settingsManager.Current.FacebookAppSecret);
			if (!result.IsSuccessful)
			{
				// TODO: deal with this
			}
			
			// persist result

			// need the returnUrl to eventually land them where they started
			return RedirectToAction("ExternalLoginCallback", new { returnUrl = "/" });
		}

		public async Task<IActionResult> GoogleCallback()
		{
			throw new NotImplementedException();
		}

		public async Task<IActionResult> MicrosoftCallback()
		{
			throw new NotImplementedException();
		}

		public async Task<IActionResult> OAuth2Callback()
		{
			throw new NotImplementedException();
		}
	}
}