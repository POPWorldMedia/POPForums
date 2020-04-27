using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class HomeController : Controller
	{
		public HomeController(IForumService forumService, IUserService userService, IUserSessionService userSessionService, IUserRetrievalShim userRetrievalShim)
		{
			_forumService = forumService;
			_userService = userService;
			_userSessionService = userSessionService;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Home";

		private readonly IForumService _forumService;
		private readonly IUserService _userService;
		private readonly IUserSessionService _userSessionService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		public async Task<ViewResult> Index()
		{
			ViewBag.OnlineUsers = await _userService.GetUsersOnline();
			var sessionCount = await _userSessionService.GetTotalSessionCount();
			ViewBag.TotalUsers = sessionCount.ToString("N0");
			ViewBag.TopicCount = _forumService.GetAggregateTopicCount().Result.ToString("N0");
			ViewBag.PostCount = _forumService.GetAggregatePostCount().Result.ToString("N0");
			var registeredUsers = await _userService.GetTotalUsers();
			ViewBag.RegisteredUsers = registeredUsers.ToString("N0");
			var user = _userRetrievalShim.GetUser();
			ViewBag.SitemapUrl = this.FullUrlHelper("Index", SitemapController.Name);
			return View(await _forumService.GetCategorizedForumContainerFilteredForUser(user));
		}
	}
}
