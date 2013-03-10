using System.Web.Mvc;
using System.Web.WebPages;
using Ninject;
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
			var container = PopForumsActivation.Kernel;
			ForumService = container.Get<IForumService>();
			UserService = container.Get<IUserService>();
			UserSessionService = container.Get<IUserSessionService>();
		}

		protected internal ForumHomeController(IForumService forumService, IUserService userService, IUserSessionService userSessionService)
		{
			ForumService = forumService;
			UserService = userService;
			UserSessionService = userSessionService;
		}

		public static string Name = "ForumHome";

		public IForumService ForumService { get; private set; }
		public IUserService UserService { get; private set; }
		public IUserSessionService UserSessionService { get; private set; }

		public ViewResult Index()
		{
			ViewBag.OnlineUsers = UserService.GetUsersOnline();
			ViewBag.TotalUsers = UserSessionService.GetTotalSessionCount().ToString("N0");
			ViewBag.TopicCount = ForumService.GetAggregateTopicCount().ToString("N0");
			ViewBag.PostCount = ForumService.GetAggregatePostCount().ToString("N0");
			ViewBag.RegisteredUsers = UserService.GetTotalUsers().ToString("N0");
			return View(ForumService.GetCategorizedForumContainerFilteredForUser(this.CurrentUser()));
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
