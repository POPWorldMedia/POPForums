using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
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
		public async Task<RedirectToActionResult> TogglePin(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				throw new Exception($"Topic with ID {id} not found. Can't pin/unpin.");
			var user = _userRetrievalShim.GetUser();
			if (topic.IsPinned)
				await _topicService.UnpinTopic(topic, user);
			else
				await _topicService.PinTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public async Task<RedirectToActionResult> ToggleClosed(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				throw new Exception($"Topic with ID {id} not found. Can't open/close.");
			var user = _userRetrievalShim.GetUser();
			if (topic.IsClosed)
				await _topicService.OpenTopic(topic, user);
			else
				await _topicService.CloseTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public async Task<RedirectToActionResult> ToggleDeleted(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				throw new Exception($"Topic with ID {id} not found. Can't delete/undelete.");
			var user = _userRetrievalShim.GetUser();
			if (topic.IsDeleted)
				await _topicService.UndeleteTopic(topic, user);
			else
				await _topicService.DeleteTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public async Task<RedirectToActionResult> UpdateTopic(IFormCollection collection)
		{
			int topicID;
			if (!int.TryParse(collection["TopicID"], out topicID))
				throw new Exception("Parse TopicID fail.");
			var topic = await _topicService.Get(topicID);
			if (topic == null)
				throw new Exception($"Topic with ID {topicID} not found. Can't update.");
			var user = _userRetrievalShim.GetUser();
			var newTitle = collection["NewTitle"];
			int forumID;
			if (!int.TryParse(collection["NewForum"], out forumID))
				throw new Exception("Parse ForumID fail.");
			var forum = await _forumService.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum with ID {forumID} not found. Can't update.");
			await _topicService.UpdateTitleAndForum(topic, forum, newTitle, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public async Task<RedirectToActionResult> UndeletePost(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				throw new Exception($"Post with ID {id} not found. Can't undelete.");
			var user = _userRetrievalShim.GetUser();
			await _postService.Undelete(post, user);
			return RedirectToAction("PostLink", "Forum", new { id = post.PostID });
		}

		public async Task<ViewResult> TopicModerationLog(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				throw new Exception($"There is no topic with ID {id} to obtain a moderation log for.");
			var log = await _moderationLogService.GetLog(topic, true);
			return View(log);
		}

		public async Task<ViewResult> PostModerationLog(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				throw new Exception($"There is no post with ID {id} to obtain a moderation log for.");
			var log = await _moderationLogService.GetLog(post);
			return View(log);
		}

		[HttpPost]
		public async Task<RedirectToActionResult> DeleteTopicPermanently(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				throw new Exception($"Topic with ID {id} not found. Can't undelete.");
			var user = _userRetrievalShim.GetUser();
			var forum = await _forumService.Get(topic.ForumID);
			await _topicService.HardDeleteTopic(topic, user);
			return RedirectToAction("Index", "Forum", new { urlName = forum.UrlName });
		}
	}
}
