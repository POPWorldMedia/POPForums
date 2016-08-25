using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Moderator)]
	[Area("Forums")]
	public class ModeratorController : Controller
	{
		public ModeratorController(ITopicService topicService, IForumService forumService, IPostService postService, IModerationLogService moderationLogService, IUserRetrievalShim userRetrievalShim)
		{
			_topicService = topicService;
			_forumService = forumService;
			_postService = postService;
			_moderationLogService = moderationLogService;
			_userRetrievalShim = userRetrievalShim;
		}

		private readonly ITopicService _topicService;
		private readonly IForumService _forumService;
		private readonly IPostService _postService;
		private readonly IModerationLogService _moderationLogService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		[HttpPost]
		public RedirectToActionResult TogglePin(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't pin/unpin.", id));
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (topic.IsPinned)
				_topicService.UnpinTopic(topic, user);
			else
				_topicService.PinTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToActionResult ToggleClosed(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't open/close.", id));
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (topic.IsClosed)
				_topicService.OpenTopic(topic, user);
			else
				_topicService.CloseTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToActionResult ToggleDeleted(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't delete/undelete.", id));
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (topic.IsDeleted)
				_topicService.UndeleteTopic(topic, user);
			else
				_topicService.DeleteTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToActionResult UpdateTopic(FormCollection collection)
		{
			int topicID;
			if (!int.TryParse(collection["TopicID"], out topicID))
				throw new Exception("Parse TopicID fail.");
			var topic = _topicService.Get(topicID);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't update.", topicID));
			var user = _userRetrievalShim.GetUser(HttpContext);
			var newTitle = collection["NewTitle"];
			int forumID;
			if (!int.TryParse(collection["NewForum"], out forumID))
				throw new Exception("Parse ForumID fail.");
			var forum = _forumService.Get(forumID);
			if (forum == null)
				throw new Exception(String.Format("Forum with ID {0} not found. Can't update.", forumID));
			_topicService.UpdateTitleAndForum(topic, forum, newTitle, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToActionResult UndeletePost(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				throw new Exception(String.Format("Post with ID {0} not found. Can't undelete.", id));
			var user = _userRetrievalShim.GetUser(HttpContext);
			_postService.Undelete(post, user);
			return RedirectToAction("PostLink", "Forum", new { id = post.PostID });
		}

		public ViewResult TopicModerationLog(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("There is no topic with ID {0} to obtain a moderation log for.", id));
			var log = _moderationLogService.GetLog(topic, true);
			return View(log);
		}

		public ViewResult PostModerationLog(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				throw new Exception(String.Format("There is no post with ID {0} to obtain a moderation log for.", id));
			var log = _moderationLogService.GetLog(post);
			return View(log);
		}

		[HttpPost]
		public RedirectToActionResult DeleteTopicPermanently(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't undelete.", id));
			var user = _userRetrievalShim.GetUser(HttpContext);
			var forum = _forumService.Get(topic.ForumID);
			_topicService.HardDeleteTopic(topic, user);
			return RedirectToAction("Index", "Forum", new { urlName = forum.UrlName });
		}
	}
}
