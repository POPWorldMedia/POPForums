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
			FavoriteTopicService = container.Get<IFavoriteTopicService>();
			ForumService = container.Get<IForumService>();
			LastReadService = container.Get<ILastReadService>();
			TopicService = container.Get<ITopicService>();
		}

		protected internal FavoritesController(IFavoriteTopicService favoriteTopicService, IForumService forumService, ILastReadService lastReadService, ITopicService topicService)
		{
			FavoriteTopicService = favoriteTopicService;
			ForumService = forumService;
			LastReadService = lastReadService;
			TopicService = topicService;
		}

		public IFavoriteTopicService FavoriteTopicService { get; private set; }
		public IForumService ForumService { get; private set; }
		public ILastReadService LastReadService { get; private set; }
		public ITopicService TopicService { get; private set; }

		public static string Name = "Favorites";

		public ViewResult Topics(int page = 1)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View();
			PagerContext pagerContext;
			var topics = FavoriteTopicService.GetTopics(user, page, out pagerContext);
			var titles = ForumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			LastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public RedirectToRouteResult RemoveFavorite(int id)
		{
			var user = this.CurrentUser();
			var topic = TopicService.Get(id);
			FavoriteTopicService.RemoveFavoriteTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleFavorite(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return Json(new BasicJsonMessage { Message = Resources.NotLoggedIn, Result = false });
			var topic = TopicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (FavoriteTopicService.IsTopicFavorite(user, topic))
			{
				FavoriteTopicService.RemoveFavoriteTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isFavorite = false }, Result = true });
			}
			FavoriteTopicService.AddFavoriteTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isFavorite = true }, Result = true });
		}
	}
}
