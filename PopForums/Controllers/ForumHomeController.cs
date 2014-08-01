using System.Web.Mvc;
using System.Web.WebPages;
using PopForums.Extensions;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	// NOTE: Controllers in POP Forums use two constructors, with the public version using manual dependency injection, 
	// and the protected internal for testing purposes. This arrangment means that POP Forums no longer requires you to 
	// set a DI container for your entire app.

	public class ForumHomeController : Controller
	{
		public ForumHomeController()
		{
			var serviceLocator = PopForumsActivation.ServiceLocator;
			_forumService = serviceLocator.GetInstance<IForumService>();
			_userService = serviceLocator.GetInstance<IUserService>();
			_userSessionService = serviceLocator.GetInstance<IUserSessionService>();
		}

		protected internal ForumHomeController(IForumService forumService, IUserService userService, IUserSessionService userSessionService)
		{
			_forumService = forumService;
			_userService = userService;
			_userSessionService = userSessionService;
		}

		public static string Name = "ForumHome";

		private readonly IForumService _forumService;
		private readonly IUserService _userService;
		private readonly IUserSessionService _userSessionService;

		public ViewResult Index()
		{
			ViewBag.OnlineUsers = _userService.GetUsersOnline();
			ViewBag.TotalUsers = _userSessionService.GetTotalSessionCount().ToString("N0");
			ViewBag.TopicCount = _forumService.GetAggregateTopicCount().ToString("N0");
			ViewBag.PostCount = _forumService.GetAggregatePostCount().ToString("N0");
			ViewBag.RegisteredUsers = _userService.GetTotalUsers().ToString("N0");
			return View(_forumService.GetCategorizedForumContainerFilteredForUser(this.CurrentUser()));
		}

		public RedirectToRouteResult SwitchView(bool mobile)
		{
			if (Request.Browser.IsMobileDevice == mobile)
				HttpContext.ClearOverriddenBrowser();
			else
				HttpContext.SetOverriddenBrowser(mobile ? BrowserOverride.Mobile : BrowserOverride.Desktop);

			return RedirectToAction("Index");
		}
	}
}
