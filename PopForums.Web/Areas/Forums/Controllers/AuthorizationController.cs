using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;
using PopForums.Web.Extensions;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class AuthorizationController : Controller
	{
		// TODO: owincontext and/or external auth
		public AuthorizationController(IUserService userService, 
			//IOwinContext owinContext, IExternalAuthentication externalAuthentication, 
			IUserAssociationManager userAssociationManager, IUserRetrievalShim userRetrievalShim)
		{
			_userService = userService;
			//_owinContext = owinContext;
			//_externalAuthentication = externalAuthentication;
			_userAssociationManager = userAssociationManager;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Authorization";

		private readonly IUserService _userService;
		//private readonly IOwinContext _owinContext;
		//private readonly IExternalAuthentication _externalAuthentication;
		private readonly IUserAssociationManager _userAssociationManager;
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
				await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		//[HttpPost]
		//public async Task<JsonResult> LoginAndAssociate(string email, string password, bool persistCookie)
		//{
		//	if (_userService.Login(email, password, persistCookie, HttpContext))
		//	{
		//		var user = _userService.GetUserByEmail(email);
		//		var authentication = _owinContext.Authentication;
		//		var authResult = await _externalAuthentication.GetAuthenticationResult(authentication);
		//		if (authResult != null)
		//			_userAssociationManager.Associate(user, authResult, HttpContext.Request.UserHostAddress);
		//		return Json(new BasicJsonMessage { Result = true });
		//	}

		//	return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		//}

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public ActionResult ExternalLogin(string provider, string returnUrl)
		//{
		//	return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Authorization", new { loginProvider = provider, ReturnUrl = returnUrl, area = "PopForums" }));
		//}

		//public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
		//{
		//	var authentication = _owinContext.Authentication;
		//	var authResult = await _externalAuthentication.GetAuthenticationResult(authentication);
		//	if (authResult == null)
		//		return RedirectToAction("Login", "Account", new { error = Resources.ExpiredLogin });
		//	var matchResult = _userAssociationManager.ExternalUserAssociationCheck(authResult, HttpContext.Request.UserHostAddress);
		//	if (matchResult.Successful)
		//	{
		//		_userService.Login(matchResult.User, true, HttpContext);
		//		return Redirect(returnUrl);
		//	}
		//	ViewBag.Referrer = returnUrl;
		//	return View();
		//}
	}
}
