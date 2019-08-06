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
using PopForums.Mvc.Areas.Forums.Models;
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
		public IActionResult ExternalLogin(string provider, string returnUrl)
		{
			var state = _stateHashingService.SetCookieAndReturnHash();
			string redirect;
			ProviderType providerType;
			switch (provider.ToLower())
			{
				case "facebook":
					var facebookRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, state, _settingsManager.Current.FacebookAppID);
					providerType = ProviderType.Facebook;
					break;
				case "google":
					var googleRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Google, googleRedirect, state, _settingsManager.Current.GoogleClientId);
					providerType = ProviderType.Google;
					break;
				case "microsoft":
					var msftRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Microsoft, msftRedirect, state, _settingsManager.Current.MicrosoftClientID);
					providerType = ProviderType.Microsoft;
					break;
				case "oauth2":
					var oauthRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
					var linkGenerator = new OAuth2LoginUrlGenerator();
					var oauthClaims = new List<string>(new[] { "openid", "email" });
					redirect = linkGenerator.GetUrl(_settingsManager.Current.OAuth2LoginUrl, _settingsManager.Current.OAuth2ClientID, oauthRedirect, state, oauthClaims);
					providerType = ProviderType.OAuth2;
					break;
				default: throw new NotImplementedException($"The external login \"{provider}\" is not configured.");
			}

			var loginState = new ExternalLoginState {ProviderType = providerType, ReturnUrl = returnUrl };
			_externalLoginTempService.Persist(loginState);
			return Redirect(redirect);
		}

		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			if (remoteError != null)
			{
				// TODO: deal with this
			}
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			var loginState = _externalLoginTempService.Read();
			var externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.CallbackResult.ResultData.ID, loginState.CallbackResult.ResultData.Name);
			var matchResult = _externalUserAssociationManager.ExternalUserAssociationCheck(externalLoginInfo, ip);
			if (matchResult.Successful)
			{
				_userService.Login(matchResult.User, ip);
				_externalLoginTempService.Remove();
				await PerformSignInAsync(matchResult.User, HttpContext);
				return Redirect(returnUrl);
			}
			ViewBag.Referrer = returnUrl;
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> LoginAndAssociate(string email, string password)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			if (_userService.Login(email, password, ip, out var user))
			{
				var loginState = _externalLoginTempService.Read();
				if (loginState != null)
				{
					var externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.CallbackResult.ResultData.ID, loginState.CallbackResult.ResultData.Name);
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
			// TODO: refactor here, you can get the provider and conditionally use the right callback
			// TODO: delete the old twitter settings, also from tests
			var facebookRedirect = this.FullUrlHelper(nameof(FacebookCallback), Name);
			var result = await _facebookCallbackProcessor.VerifyCallback(facebookRedirect, _settingsManager.Current.FacebookAppID, _settingsManager.Current.FacebookAppSecret);
			if (!result.IsSuccessful)
			{
				// TODO: deal with this
			}
			// persist result
			var loginState = _externalLoginTempService.Read();
			// TODO: what if it's missing?
			loginState.CallbackResult = result;
			_externalLoginTempService.Persist(loginState);

			// need the returnUrl to eventually land them where they started
			return RedirectToAction("ExternalLoginCallback", new { returnUrl = loginState.ReturnUrl });
		}
	}
}