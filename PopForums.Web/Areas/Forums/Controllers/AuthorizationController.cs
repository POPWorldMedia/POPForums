using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.Mvc;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class AuthorizationController : Controller
	{
		public AuthorizationController(IUserService userService, IExternalUserAssociationManager externalUserAssociationManager, IUserRetrievalShim userRetrievalShim)
		{
			_userService = userService;
			_externalUserAssociationManager = externalUserAssociationManager;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Authorization";

		private readonly IUserService _userService;
		private readonly IExternalUserAssociationManager _externalUserAssociationManager;
		private readonly IUserRetrievalShim _userRetrievalShim;

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
			var user = _userRetrievalShim.GetUser(HttpContext);
			_userService.Logout(user, HttpContext.Connection.RemoteIpAddress.ToString());
			await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect(link);
		}

		[HttpPost]
		public async Task<JsonResult> LogoutAsync()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			_userService.Logout(user, HttpContext.Connection.RemoteIpAddress.ToString());
			await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public async Task<IActionResult> Login(string email, string password, bool persistCookie)
		{
			User user;
			if (_userService.Login(email, password, persistCookie, HttpContext.Connection.RemoteIpAddress.ToString(), out user))
			{
				await PerformSignInAsync(persistCookie, user);
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		private async Task PerformSignInAsync(bool persistCookie, User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Name)
			};

			var props = new AuthenticationProperties
			{
				IsPersistent = persistCookie,
				ExpiresUtc = DateTime.UtcNow.AddYears(1)
			};

			var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			await
				HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id),
					props);
		}

		[HttpPost]
		public async Task<JsonResult> LoginAndAssociate(string email, string password, bool persistCookie)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
			User user;
            if (_userService.Login(email, password, persistCookie, ip, out user))
			{
				var authResult = await GetExternalLoginInfoAsync();
				if (authResult != null)
				{
					_externalUserAssociationManager.Associate(user, authResult, ip);
					await PerformSignInAsync(persistCookie, user);
					return Json(new BasicJsonMessage { Result = true });
				}
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			var redirectUrl = Url.Action("ExternalLoginCallback", Name, new { ReturnUrl = returnUrl });
			var properties = ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		private const string LoginProviderKey = "LoginProvider";
		private const string XsrfKey = "XsrfId";

		public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null)
		{
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			properties.Items[LoginProviderKey] = provider;
			if (userId != null)
			{
				properties.Items[XsrfKey] = userId;
			}
			return properties;
		}

		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
		{
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var info = await GetExternalLoginInfoAsync();
			if (info == null)
				return RedirectToAction("Login", "Account", new { error = Resources.ExpiredLogin });
			var email = info.ExternalPrincipal.HasClaim(x => x.Type == ClaimTypes.Email) ? info.ExternalPrincipal.FindFirst(ClaimTypes.Email).Value : null;
			var name = info.ExternalPrincipal.HasClaim(x => x.Type == ClaimTypes.Name) ? info.ExternalPrincipal.FindFirst(ClaimTypes.Name).Value : null;
			var externalAuthResult = new ExternalAuthenticationResult
			{
				Issuer = info.LoginProvider,
				Email = email,
				Name = name,
				ProviderKey = info.ProviderKey
			};
			var matchResult = _externalUserAssociationManager.ExternalUserAssociationCheck(externalAuthResult, ip);
			if (matchResult.Successful)
			{
				_userService.Login(matchResult.User, true, ip);
				await PerformSignInAsync(true, matchResult.User);
				return Redirect(returnUrl);
			}
			ViewBag.Referrer = returnUrl;
			return View();
		}

		// PopForums doesn't use the Xsrf property, because it associates claims with user accounts, not 
		// accounts with claims. For example, a user can't add a Facebook association while already logged in.
		// They must use their PF credentials after getting valid claims from the 3rd party.
		private async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
		{
			var auth = new AuthenticateContext(ExternalExternalUserAssociationManager.AuthenticationContextName);
			await HttpContext.Authentication.AuthenticateAsync(auth);
			if (auth.Principal == null || auth.Properties == null || !auth.Properties.ContainsKey(LoginProviderKey))
			{
				return null;
			}

			if (expectedXsrf != null)
			{
				if (!auth.Properties.ContainsKey(XsrfKey))
				{
					return null;
				}
				var userId = auth.Properties[XsrfKey];
				if (userId != expectedXsrf)
				{
					return null;
				}
			}

			var claim = auth.Principal.FindFirst(ClaimTypes.NameIdentifier);
			var providerKey = claim?.Value;
			var provider = auth.Properties[LoginProviderKey];
			if (providerKey == null || provider == null)
			{
				return null;
			}
			return new ExternalLoginInfo(auth.Principal, provider, providerKey, auth.Properties[LoginProviderKey]);
		}
	}
}
