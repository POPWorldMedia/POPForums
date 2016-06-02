using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class FavoritesController : Controller
	{
		public FavoritesController(IFavoriteTopicService favoriteTopicService, IForumService forumService, ILastReadService lastReadService, ITopicService topicService, IUserRetrievalShim userRetrievalShim)
		{
			_favoriteTopicService = favoriteTopicService;
			_forumService = forumService;
			_lastReadService = lastReadService;
			_topicService = topicService;
			_userRetrievalShim = userRetrievalShim;
		}

		private readonly IFavoriteTopicService _favoriteTopicService;
		private readonly IForumService _forumService;
		private readonly ILastReadService _lastReadService;
		private readonly ITopicService _topicService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		public static string Name = "Favorites";

		public ViewResult Topics(int page = 1)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View();
			PagerContext pagerContext;
			var topics = _favoriteTopicService.GetTopics(user, page, out pagerContext);
			var titles = _forumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			_lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public ActionResult RemoveFavorite(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			var topic = _topicService.Get(id);
			_favoriteTopicService.RemoveFavoriteTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleFavorite(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Json(new BasicJsonMessage { Message = Resources.NotLoggedIn, Result = false });
			var topic = _topicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (_favoriteTopicService.IsTopicFavorite(user, topic))
			{
				_favoriteTopicService.RemoveFavoriteTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isFavorite = false }, Result = true });
			}
			_favoriteTopicService.AddFavoriteTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isFavorite = true }, Result = true });
		}
	}
}
