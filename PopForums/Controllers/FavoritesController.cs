using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class FavoritesController : Controller
	{
		public FavoritesController()
		{
			var container = PopForumsActivation.Kernel;
			_favoriteTopicService = container.Get<IFavoriteTopicService>();
			_forumService = container.Get<IForumService>();
			_lastReadService = container.Get<ILastReadService>();
			_topicService = container.Get<ITopicService>();
		}

		protected internal FavoritesController(IFavoriteTopicService favoriteTopicService, IForumService forumService, ILastReadService lastReadService, ITopicService topicService)
		{
			_favoriteTopicService = favoriteTopicService;
			_forumService = forumService;
			_lastReadService = lastReadService;
			_topicService = topicService;
		}

		private readonly IFavoriteTopicService _favoriteTopicService;
		private readonly IForumService _forumService;
		private readonly ILastReadService _lastReadService;
		private readonly ITopicService _topicService;

		public static string Name = "Favorites";

		public ViewResult Topics(int page = 1)
		{
			var user = this.CurrentUser();
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
		public RedirectToRouteResult RemoveFavorite(int id)
		{
			var user = this.CurrentUser();
			var topic = _topicService.Get(id);
			_favoriteTopicService.RemoveFavoriteTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleFavorite(int id)
		{
			var user = this.CurrentUser();
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
