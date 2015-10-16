using Microsoft.AspNet.Mvc;
using PopForums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class HomeController : Controller
	{
		public HomeController(IForumService forumService, IUserService userService, IUserSessionService userSessionService)
		{
			_forumService = forumService;
			_userService = userService;
			_userSessionService = userSessionService;
		}

		public static string Name = "Home";

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
			// TODO: need a user here!
			return View(_forumService.GetCategorizedForumContainerFilteredForUser(null));
		}
	}
}
