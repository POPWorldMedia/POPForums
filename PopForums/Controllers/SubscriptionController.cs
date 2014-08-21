using System;
using System.Web.Mvc;
using PopForums.Configuration.DependencyResolution;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Controllers
{
	public class SubscriptionController : Controller
	{
		public SubscriptionController()
		{
			var serviceLocator = StructuremapMvc.StructureMapDependencyScope;
			_subService = serviceLocator.GetInstance<ISubscribedTopicsService>();
			_topicService = serviceLocator.GetInstance<ITopicService>();
			_userService = serviceLocator.GetInstance<IUserService>();
			_lastReadService = serviceLocator.GetInstance<ILastReadService>();
			_forumService = serviceLocator.GetInstance<IForumService>();
		}

		protected internal SubscriptionController(ISubscribedTopicsService subService, ITopicService topicService, IUserService userService, ILastReadService lastReadService, IForumService forumService)
		{
			_subService = subService;
			_topicService = topicService;
			_userService = userService;
			_lastReadService = lastReadService;
			_forumService = forumService;
		}

		public static string Name = "Subscription";

		private readonly ISubscribedTopicsService _subService;
		private readonly ITopicService _topicService;
		private readonly IUserService _userService;
		private readonly ILastReadService _lastReadService;
		private readonly IForumService _forumService;

		public ViewResult Topics(int page = 1)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View();
			PagerContext pagerContext;
			var topics = _subService.GetTopics(user, page, out pagerContext);
			var titles = _forumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			_lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		public ViewResult Unsubscribe(int topicID, string authKey)
		{
			var container = new TopicUnsubscribeContainer {User = null, Topic = null};
			Guid parsedKey;
			if (!Guid.TryParse(authKey, out parsedKey))
				return View(container);
			container.User = _userService.GetUserByAuhtorizationKey(parsedKey);
			container.Topic = _topicService.Get(topicID);
			_subService.TryRemoveSubscribedTopic(container.User, container.Topic);
			return View(container);
		}

		[HttpPost]
		public RedirectToRouteResult Unsubscribe(int id)
		{
			var user = this.CurrentUser();
			var topic = _topicService.Get(id);
			_subService.TryRemoveSubscribedTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleSubscription(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return Json(new BasicJsonMessage{ Message = Resources.LoginToPost, Result = false });
			var topic = _topicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (_subService.IsTopicSubscribed(user, topic))
			{
				_subService.RemoveSubscribedTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isSubscribed = false }, Result = true });
			}
			_subService.AddSubscribedTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isSubscribed = true }, Result = true });
		}
	}
}
