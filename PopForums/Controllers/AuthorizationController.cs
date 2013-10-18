using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin;
using Ninject;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class AuthorizationController : Controller
	{
		public AuthorizationController()
		{
			var container = PopForumsActivation.Kernel;
			_userService = container.Get<IUserService>();
			_owinContext = container.Get<IOwinContext>();
			_externalAuthentication = container.Get<IExternalAuthentication>();
			_userAssociationManager = container.Get<IUserAssociationManager>();
		}

		protected internal AuthorizationController(IUserService userService, IOwinContext owinContext, IExternalAuthentication externalAuthentication, IUserAssociationManager userAssociationManager)
		{
			_userService = userService;
			_owinContext = owinContext;
			_externalAuthentication = externalAuthentication;
			_userAssociationManager = userAssociationManager;
		}

		public static string Name = "Authorization";

		private readonly IUserService _userService;
		private readonly IOwinContext _owinContext;
		private readonly IExternalAuthentication _externalAuthentication;
		private readonly IUserAssociationManager _userAssociationManager;

		[HttpGet]
		public RedirectResult Logout()
		{
			string link;
			if (Request == null || Request.UrlReferrer == null || Request.Url == null)
				link = Url.Action("Index", ForumHomeController.Name);
			else
			{
				link = Request.UrlReferrer.ToString();
				if (!link.Contains(Request.Url.Host))
					link = Url.Action("Index", ForumHomeController.Name);
			}
			var user = this.CurrentUser();
			_userService.Logout(user, HttpContext.Request.UserHostAddress);
			return Redirect(link);
		}

		[HttpPost]
		public JsonResult LogoutAsync()
		{
			var user = this.CurrentUser();
			_userService.Logout(user, HttpContext.Request.UserHostAddress);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult Login(string email, string password, bool persistCookie)
		{
			if (_userService.Login(email, password, persistCookie, HttpContext))
			{
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		[HttpPost]
		public async Task<JsonResult> LoginAndAssociate(string email, string password, bool persistCookie)
		{
			if (_userService.Login(email, password, persistCookie, HttpContext))
			{
				var user = _userService.GetUserByEmail(email);
				var authentication = _owinContext.Authentication;
				var authResult = await _externalAuthentication.GetAuthenticationResult(authentication);
				if (authResult != null)
					_userAssociationManager.Associate(user, authResult);
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ExternalLogin(string provider, string returnUrl)
		{
			return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Authorization", new { loginProvider = provider, ReturnUrl = returnUrl }));
		}

		public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
		{
			var authentication = _owinContext.Authentication;
			var authResult = await _externalAuthentication.GetAuthenticationResult(authentication);
			if (authResult == null)
				return RedirectToAction("Login", "Account", new { error = Resources.ExpiredLogin });
			var matchResult = _userAssociationManager.ExternalUserAssociationCheck(authResult);
			if (matchResult.Successful)
			{
				_userService.Login(matchResult.User, true, HttpContext);
				return Redirect(returnUrl);
			}
			ViewBag.Referrer = returnUrl;
			return View();
		}
	}
}
