using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
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
			UserService = container.Get<IUserService>();
		}

		protected internal AuthorizationController(IUserService userService)
		{
			UserService = userService;
		}

		public static string Name = "Authorization";

		public IUserService UserService { get; set; }

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
			UserService.Logout(user, HttpContext.Request.UserHostAddress);
			return Redirect(link);
		}

		[HttpPost]
		public JsonResult LogoutAsync()
		{
			var user = this.CurrentUser();
			UserService.Logout(user, HttpContext.Request.UserHostAddress);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult Login(string email, string password, bool persistCookie)
		{
			if (UserService.Login(email, password, persistCookie, HttpContext))
			{
				return Json(new BasicJsonMessage { Result = true });
			}

			return Json(new BasicJsonMessage { Result = false, Message = Resources.LoginBad });
		}
	}
}
