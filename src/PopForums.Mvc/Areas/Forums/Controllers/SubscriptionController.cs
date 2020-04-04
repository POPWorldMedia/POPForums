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
		public SubscriptionController(ISubscribedTopicsService subService, ITopicService topicService, IUserService userService, ILastReadService lastReadService, IForumService forumService, IUserRetrievalShim userRetrievalShim, IProfileService profileService)
		{
			_subService = subService;
			_topicService = topicService;
			_userService = userService;
			_lastReadService = lastReadService;
			_forumService = forumService;
			_userRetrievalShim = userRetrievalShim;
			_profileService = profileService;
		}

		public static string Name = "Subscription";

		private readonly ISubscribedTopicsService _subService;
		private readonly ITopicService _topicService;
		private readonly IUserService _userService;
		private readonly ILastReadService _lastReadService;
		private readonly IForumService _forumService;
	    private readonly IUserRetrievalShim _userRetrievalShim;
	    private readonly IProfileService _profileService;

	    public async Task<ViewResult> Topics(int pageNumber = 1)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return View();
			var (topics, pagerContext) = await _subService.GetTopics(user, pageNumber);
			var titles = _forumService.GetAllForumTitles();
			var container = new PagedTopicContainer { PagerContext = pagerContext, Topics = topics, ForumTitles = titles };
			await _lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		public async Task<ViewResult> Unsubscribe(int topicID, int userID, string hash)
		{
			var container = new TopicUnsubscribeContainer { User = null, Topic = null };
			container.User = await _userService.GetUser(userID);
			container.Topic = await _topicService.Get(topicID);
			var unsubscribeHash = _profileService.GetUnsubscribeHash(container.User);
			if (unsubscribeHash != hash)
				return View(container);
			await _subService.TryRemoveSubscribedTopic(container.User, container.Topic);
			return View(container);
		}

		[HttpPost]
		public async Task<ActionResult> Unsubscribe(int id)
		{
			var user = _userRetrievalShim.GetUser();
			var topic = await _topicService.Get(id);
			await _subService.TryRemoveSubscribedTopic(user, topic);
			return RedirectToAction("Topics");
		}

		[HttpPost]
		public async Task<JsonResult> ToggleSubscription(int id)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return Json(new BasicJsonMessage { Message = Resources.LoginToPost, Result = false });
			var topic = await _topicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			if (await _subService.IsTopicSubscribed(user, topic))
			{
				await _subService.RemoveSubscribedTopic(user, topic);
				return Json(new BasicJsonMessage { Data = new { isSubscribed = false }, Result = true });
			}
			await _subService.AddSubscribedTopic(user, topic);
			return Json(new BasicJsonMessage { Data = new { isSubscribed = true }, Result = true });
		}
	}
}
