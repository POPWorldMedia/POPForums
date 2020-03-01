using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
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

		public async Task<ViewResult> Topics(int pageNumber = 1)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return View();
			var (topics, pagerContext) = await _favoriteTopicService.GetTopics(user, pageNumber);
			var titles = _forumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			await _lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public async Task<ActionResult> RemoveFavorite(int id)
		{
			var user = _userRetrievalShim.GetUser();
			var topic = await _topicService.Get(id);
			await _favoriteTopicService.RemoveFavoriteTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public async Task<JsonResult> ToggleFavorite(int id)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return Json(new BasicJsonMessage { Message = Resources.NotLoggedIn, Result = false });
			var topic = await _topicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (await _favoriteTopicService.IsTopicFavorite(user, topic))
			{
				await _favoriteTopicService.RemoveFavoriteTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isFavorite = false }, Result = true });
			}
			await _favoriteTopicService.AddFavoriteTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isFavorite = true }, Result = true });
		}
	}
}
