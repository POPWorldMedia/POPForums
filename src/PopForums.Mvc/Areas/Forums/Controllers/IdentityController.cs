using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PopForums.Configuration;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Mvc.Areas.Forums.Services;
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
		private readonly IExternalLoginTempService _externalLoginTempService;

		public IdentityController(ILoginLinkFactory loginLinkFactory, IStateHashingService stateHashingService, ISettingsManager settingsManager, IFacebookCallbackProcessor facebookCallbackProcessor, IGoogleCallbackProcessor googleCallbackProcessor, IMicrosoftCallbackProcessor microsoftCallbackProcessor, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor, IExternalUserAssociationManager externalUserAssociationManager, IUserService userService, IExternalLoginTempService externalLoginTempService)
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
			_externalLoginTempService = externalLoginTempService;
		}

		public string Name = "Identity";

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
			var authResult = _externalLoginTempService.Read();
			var externalLoginInfo = new ExternalLoginInfo(authResult.ProviderType.ToString(), authResult.ResultData.ID, authResult.ResultData.Name);
			var matchResult = _externalUserAssociationManager.ExternalUserAssociationCheck(externalLoginInfo, ip);
			if (matchResult.Successful)
			{
				_userService.Login(matchResult.User, ip);
				_externalLoginTempService.Remove();
				await PerformSignInAsync(matchResult.User, HttpContext);
				return Redirect(returnUrl);
			}

			ViewBag.Referrer = returnUrl;
			return View(); // use the view from the old auth controller
		}

		[HttpPost]
		public async Task<JsonResult> LoginAndAssociate(string email, string password)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			if (_userService.Login(email, password, ip, out var user))
			{
				var authResult = _externalLoginTempService.Read();
				if (authResult != null)
				{
					var externalLoginInfo = new ExternalLoginInfo(authResult.ProviderType.ToString(), authResult.ResultData.ID, authResult.ResultData.Name);
					_externalUserAssociationManager.Associate(user, externalLoginInfo, ip);
					_externalLoginTempService.Remove();
					await PerformSignInAsync(user, HttpContext);
					return Json(new BasicJsonMessage { Result = true });
				}
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
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
			_externalLoginTempService.Persist(result);

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