using System;
using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class SubscriptionController : Controller
	{
		public SubscriptionController()
		{
			var container = PopForumsActivation.Kernel;
			SubService = container.Get<ISubscribedTopicsService>();
			TopicService = container.Get<ITopicService>();
			UserService = container.Get<IUserService>();
			LastReadService = container.Get<ILastReadService>();
			ForumService = container.Get<IForumService>();
		}

		protected internal SubscriptionController(ISubscribedTopicsService subService, ITopicService topicService, IUserService userService, ILastReadService lastReadService, IForumService forumService)
		{
			SubService = subService;
			TopicService = topicService;
			UserService = userService;
			LastReadService = lastReadService;
			ForumService = forumService;
		}

		public static string Name = "Subscription";

		public ISubscribedTopicsService SubService { get; private set; }
		public ITopicService TopicService { get; private set; }
		public IUserService UserService { get; private set; }
		public ILastReadService LastReadService { get; private set; }
		public IForumService ForumService { get; private set; }

		public ViewResult Topics(int page = 1)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View();
			PagerContext pagerContext;
			var topics = SubService.GetTopics(user, page, out pagerContext);
			var titles = ForumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			LastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		public ViewResult Unsubscribe(int topicID, string authKey)
		{
			var container = new TopicUnsubscribeContainer {User = null, Topic = null};
			Guid parsedKey;
			if (!Guid.TryParse(authKey, out parsedKey))
				return View(container);
			container.User = UserService.GetUserByAuhtorizationKey(parsedKey);
			container.Topic = TopicService.Get(topicID);
			SubService.TryRemoveSubscribedTopic(container.User, container.Topic);
			return View(container);
		}

		[HttpPost]
		public RedirectToRouteResult Unsubscribe(int id)
		{
			var user = this.CurrentUser();
			var topic = TopicService.Get(id);
			SubService.TryRemoveSubscribedTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleSubscription(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return Json(new BasicJsonMessage{ Message = Resources.LoginToPost, Result = false });
			var topic = TopicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (SubService.IsTopicSubscribed(user, topic))
			{
				SubService.RemoveSubscribedTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isSubscribed = false }, Result = true });
			}
			SubService.AddSubscribedTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isSubscribed = true }, Result = true });
		}
	}
}
