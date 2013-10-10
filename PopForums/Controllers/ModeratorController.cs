using System;
using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	[Moderator]
	public class ModeratorController : Controller
	{
		public ModeratorController()
		{
			var container = PopForumsActivation.Kernel;
			_topicService = container.Get<ITopicService>();
			_forumService = container.Get<IForumService>();
			_postService = container.Get<IPostService>();
			_moderationLogService = container.Get<IModerationLogService>();
		}

		protected internal ModeratorController(ITopicService topicService, IForumService forumService, IPostService postService, IModerationLogService moderationLogService)
		{
			_topicService = topicService;
			_forumService = forumService;
			_postService = postService;
			_moderationLogService = moderationLogService;
		}

		private readonly ITopicService _topicService;
		private readonly IForumService _forumService;
		private readonly IPostService _postService;
		private readonly IModerationLogService _moderationLogService;

		[HttpPost]
		public RedirectToRouteResult TogglePin(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't pin/unpin.", id));
			var user = this.CurrentUser();
			if (topic.IsPinned)
				_topicService.UnpinTopic(topic, user);
			else
				_topicService.PinTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new {id = topic.UrlName});
		}

		[HttpPost]
		public RedirectToRouteResult ToggleClosed(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't open/close.", id));
			var user = this.CurrentUser();
			if (topic.IsClosed)
				_topicService.OpenTopic(topic, user);
			else
				_topicService.CloseTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToRouteResult ToggleDeleted(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't delete/undelete.", id));
			var user = this.CurrentUser();
			if (topic.IsDeleted)
				_topicService.UndeleteTopic(topic, user);
			else
				_topicService.DeleteTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToRouteResult UpdateTopic(FormCollection collection)
		{
			int topicID;
			if (!int.TryParse(collection["TopicID"], out topicID))
				throw new Exception("Parse TopicID fail.");
			var topic = _topicService.Get(topicID);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't update.", topicID));
			var user = this.CurrentUser();
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
		public RedirectToRouteResult UndeletePost(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				throw new Exception(String.Format("Post with ID {0} not found. Can't undelete.", id));
			var user = this.CurrentUser();
			_postService.Undelete(post, user);
			return RedirectToAction("PostLink", "Forum", new {id = post.PostID});
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
	}
}
