using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class SubscriptionController : Controller
    {
		public SubscriptionController(ISubscribedTopicsService subService, ITopicService topicService, IUserService userService, ILastReadService lastReadService, IForumService forumService, IUserRetrievalShim userRetrievalShim)
		{
			_subService = subService;
			_topicService = topicService;
			_userService = userService;
			_lastReadService = lastReadService;
			_forumService = forumService;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Subscription";

		private readonly ISubscribedTopicsService _subService;
		private readonly ITopicService _topicService;
		private readonly IUserService _userService;
		private readonly ILastReadService _lastReadService;
		private readonly IForumService _forumService;
	    private readonly IUserRetrievalShim _userRetrievalShim;

	    public async Task<ViewResult> Topics(int page = 1)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View();
			PagerContext pagerContext;
			var topics = _subService.GetTopics(user, page, out pagerContext);
			var titles = _forumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			await _lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		public ViewResult Unsubscribe(int topicID, string authKey)
		{
			var container = new TopicUnsubscribeContainer { User = null, Topic = null };
			Guid parsedKey;
			if (!Guid.TryParse(authKey, out parsedKey))
				return View(container);
			container.User = _userService.GetUserByAuhtorizationKey(parsedKey);
			container.Topic = _topicService.Get(topicID);
			_subService.TryRemoveSubscribedTopic(container.User, container.Topic);
			return View(container);
		}

		[HttpPost]
		public ActionResult Unsubscribe(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			var topic = _topicService.Get(id);
			_subService.TryRemoveSubscribedTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public JsonResult ToggleSubscription(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Json(new BasicJsonMessage { Message = Resources.LoginToPost, Result = false });
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
