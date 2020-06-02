using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PopForums.Configuration;
using PopForums.Extensions;
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
		private readonly IProfileService _profileService;
		private readonly IExternalLoginTempService _externalLoginTempService;
		private readonly IUserRetrievalShim _userRetrievalShim;
		private readonly ISecurityLogService _securityLogService;
		private readonly IConfig _config;
		private readonly IAutoProvisionAccountService _autoProvisionAccountService;

		public IdentityController(ILoginLinkFactory loginLinkFactory, IStateHashingService stateHashingService, ISettingsManager settingsManager, IFacebookCallbackProcessor facebookCallbackProcessor, IGoogleCallbackProcessor googleCallbackProcessor, IMicrosoftCallbackProcessor microsoftCallbackProcessor, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor, IExternalUserAssociationManager externalUserAssociationManager, IUserService userService, IProfileService profileService, IExternalLoginTempService externalLoginTempService, IUserRetrievalShim userRetrievalShim, ISecurityLogService securityLogService, IConfig config, IAutoProvisionAccountService autoProvisionAccountService = null)
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
			_profileService = profileService;
			_externalLoginTempService = externalLoginTempService;
			_userRetrievalShim = userRetrievalShim;
			_securityLogService = securityLogService;
			_config = config;
			_autoProvisionAccountService = autoProvisionAccountService;
		}

		public static string Name = "Identity";

		[PopForumsAuthorizationIgnore]
		[TypeFilter(typeof(PopForumsExternalLoginOnlyFilter))]
		[HttpPost]
		public async Task<IActionResult> Login(string email, string password)
		{
			var (result, user) = await _userService.Login(email, password, HttpContext.Connection.RemoteIpAddress.ToString());
			if (result)
			{
				await PerformSignInAsync(user, HttpContext);
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		[HttpGet]
		public async Task<RedirectResult> Logout()
		{
			string link;
			if (Request == null || string.IsNullOrWhiteSpace(Request.Headers["Referer"]))
				link = Url.Action("Index", HomeController.Name);
			else
			{
				link = Request.Headers["Referer"];
				if (!link.Contains(Request.Host.Value))
					link = Url.Action("Index", HomeController.Name);
			}
			var user = _userRetrievalShim.GetUser();
			await _userService.Logout(user, HttpContext.Connection.RemoteIpAddress.ToString());
			await HttpContext.SignOutAsync(PopForumsAuthorizationDefaults.AuthenticationScheme);
			return Redirect(link);
		}

		[HttpPost]
		public async Task<JsonResult> LogoutAsync()
		{
			var user = _userRetrievalShim.GetUser();
			await _userService.Logout(user, HttpContext.Connection.RemoteIpAddress.ToString());
			await HttpContext.SignOutAsync(PopForumsAuthorizationDefaults.AuthenticationScheme);
			return Json(new BasicJsonMessage { Result = true });
		}

		[PopForumsAuthorizationIgnore]
		[HttpPost]
		public IActionResult ExternalLogin(string provider, string returnUrl)
		{
			var state = _stateHashingService.SetCookieAndReturnHash();
			string redirect;
			ProviderType providerType;
			switch (provider.ToLower())
			{
				case "facebook":
					var facebookRedirect = this.FullUrlHelper(nameof(CallbackHandler), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, state, _settingsManager.Current.FacebookAppID);
					providerType = ProviderType.Facebook;
					break;
				case "google":
					var googleRedirect = this.FullUrlHelper(nameof(CallbackHandler), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Google, googleRedirect, state, _settingsManager.Current.GoogleClientId);
					providerType = ProviderType.Google;
					break;
				case "microsoft":
					var msftRedirect = this.FullUrlHelper(nameof(CallbackHandler), Name);
					redirect = _loginLinkFactory.GetLink(ProviderType.Microsoft, msftRedirect, state, _settingsManager.Current.MicrosoftClientID);
					providerType = ProviderType.Microsoft;
					break;
				case "oauth2":
					var oauthRedirect = this.FullUrlHelper(nameof(CallbackHandler), Name);
					var linkGenerator = new OAuth2LoginUrlGenerator();
					var oauthClaims = new List<string>(new[] { "openid", "profile", "email" });
					redirect = linkGenerator.GetUrl(_settingsManager.Current.OAuth2LoginUrl, _settingsManager.Current.OAuth2ClientID, oauthRedirect, state, oauthClaims);
					providerType = ProviderType.OAuth2;
					break;
				default: throw new NotImplementedException($"The external login \"{provider}\" is not configured.");
			}

			var loginState = new ExternalLoginState { ProviderType = providerType, ReturnUrl = returnUrl };
			_externalLoginTempService.Persist(loginState);
			return Redirect(redirect);
		}

		[PopForumsAuthorizationIgnore]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			var loginState = _externalLoginTempService.Read();
			if (loginState == null)
			{
				await _securityLogService.CreateLogEntry((User)null, null, ip, "Temp auth cookie missing on callback", SecurityLogType.ExternalAssociationCheckFailed);
				return View("ExternalError", Resources.LoginBad);
			}
			var externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.ResultData.ID, loginState.ResultData.Name);
			var matchResult = await _externalUserAssociationManager.ExternalUserAssociationCheck(externalLoginInfo, ip);
			if (matchResult.Successful)
			{
				await _userService.Login(matchResult.User, ip);
				_externalLoginTempService.Remove();
				await PerformSignInAsync(matchResult.User, HttpContext);
				return Redirect(returnUrl);
			}

			if (_config.ExternalLoginOnly)
			{
				if (_autoProvisionAccountService == null)
					throw new Exception("AutoProvisionAccountService must be registered.");

				var user = await _autoProvisionAccountService.AutoProvisionAccountAsync(loginState.ResultData.Name, loginState.ResultData.Email, ip);

				await _userService.Login(user, ip);
				externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.ResultData.ID, user.Name);
				await _externalUserAssociationManager.Associate(user, externalLoginInfo, ip);
				_externalLoginTempService.Remove();
				await PerformSignInAsync(user, HttpContext);
				return Redirect(returnUrl);
			}

			ViewBag.Referrer = returnUrl;
			return View();
		}

		[PopForumsAuthorizationIgnore]
		[TypeFilter(typeof(PopForumsExternalLoginOnlyFilter))]
		[HttpPost]
		public async Task<IActionResult> LoginAndAssociate(string email, string password)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			var (result, user) = await _userService.Login(email, password, ip);
			if (result)
			{
				var loginState = _externalLoginTempService.Read();
				if (loginState != null)
				{
					var externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.ResultData.ID, loginState.ResultData.Name);
					await _externalUserAssociationManager.Associate(user, externalLoginInfo, ip);
					_externalLoginTempService.Remove();
					await PerformSignInAsync(user, HttpContext);
					return Json(new BasicJsonMessage { Result = true });
				}
				else
				{
					await _securityLogService.CreateLogEntry((User)null, null, ip, "Temp auth cookie missing on association", SecurityLogType.ExternalAssociationCheckFailed);
					return Json(new BasicJsonMessage { Result = false, Message = Resources.Error + ": " + Resources.LoginBad });
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

		[PopForumsAuthorizationIgnore]
		public async Task<IActionResult> CallbackHandler()
		{
			var loginState = _externalLoginTempService.Read();
			if (loginState == null)
			{
				var ip = HttpContext.Connection.RemoteIpAddress.ToString();
				await _securityLogService.CreateLogEntry((User)null, null, ip, "Temp auth cookie missing on callback", SecurityLogType.ExternalAssociationCheckFailed);
				return View("ExternalError", Resources.Error + ": " + Resources.LoginBad);
			}
			var redirectUri = this.FullUrlHelper(nameof(CallbackHandler), Name);
			CallbackResult result;
			switch (loginState.ProviderType)
			{
				case ProviderType.Facebook:
					result = await _facebookCallbackProcessor.VerifyCallback(redirectUri, _settingsManager.Current.FacebookAppID, _settingsManager.Current.FacebookAppSecret);
					break;
				case ProviderType.Google:
					result = await _googleCallbackProcessor.VerifyCallback(redirectUri, _settingsManager.Current.GoogleClientId, _settingsManager.Current.GoogleClientSecret);
					break;
				case ProviderType.Microsoft:
					result = await _microsoftCallbackProcessor.VerifyCallback(redirectUri, _settingsManager.Current.MicrosoftClientID, _settingsManager.Current.MicrosoftClientSecret);
					break;
				case ProviderType.OAuth2:
					result = await _oAuth2JwtCallbackProcessor.VerifyCallback(redirectUri, _settingsManager.Current.OAuth2TokenUrl, _settingsManager.Current.OAuth2ClientID, _settingsManager.Current.OAuth2ClientSecret);
					break;
				default:
					throw new Exception($"The external login type {loginState.ProviderType} has no callback handler.");
			}
			if (!result.IsSuccessful)
			{
				return View("ExternalError", result.Message);
			}
			loginState.ResultData = result.ResultData;
			_externalLoginTempService.Persist(loginState);

			return RedirectToAction("ExternalLoginCallback", new { returnUrl = loginState.ReturnUrl });
		}
	}
}