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
			TopicService = container.Get<ITopicService>();
			ForumService = container.Get<IForumService>();
			PostService = container.Get<IPostService>();
			ModerationLogService = container.Get<IModerationLogService>();
		}

		protected internal ModeratorController(ITopicService topicService, IForumService forumService, IPostService postService, IModerationLogService moderationLogService)
		{
			TopicService = topicService;
			ForumService = forumService;
			PostService = postService;
			ModerationLogService = moderationLogService;
		}

		public ITopicService TopicService { get; private set; }
		public IForumService ForumService { get; private set; }
		public IPostService PostService { get; private set; }
		public IModerationLogService ModerationLogService { get; private set; }

		[HttpPost]
		public RedirectToRouteResult TogglePin(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't pin/unpin.", id));
			var user = this.CurrentUser();
			if (topic.IsPinned)
				TopicService.UnpinTopic(topic, user);
			else
				TopicService.PinTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new {id = topic.UrlName});
		}

		[HttpPost]
		public RedirectToRouteResult ToggleClosed(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't open/close.", id));
			var user = this.CurrentUser();
			if (topic.IsClosed)
				TopicService.OpenTopic(topic, user);
			else
				TopicService.CloseTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToRouteResult ToggleDeleted(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't delete/undelete.", id));
			var user = this.CurrentUser();
			if (topic.IsDeleted)
				TopicService.UndeleteTopic(topic, user);
			else
				TopicService.DeleteTopic(topic, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToRouteResult UpdateTopic(FormCollection collection)
		{
			int topicID;
			if (!int.TryParse(collection["TopicID"], out topicID))
				throw new Exception("Parse TopicID fail.");
			var topic = TopicService.Get(topicID);
			if (topic == null)
				throw new Exception(String.Format("Topic with ID {0} not found. Can't update.", topicID));
			var user = this.CurrentUser();
			var newTitle = collection["NewTitle"];
			int forumID;
			if (!int.TryParse(collection["NewForum"], out forumID))
				throw new Exception("Parse ForumID fail.");
			var forum = ForumService.Get(forumID);
			if (forum == null)
				throw new Exception(String.Format("Forum with ID {0} not found. Can't update.", forumID));
			TopicService.UpdateTitleAndForum(topic, forum, newTitle, user);
			return RedirectToAction("Topic", "Forum", new { id = topic.UrlName });
		}

		[HttpPost]
		public RedirectToRouteResult UndeletePost(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				throw new Exception(String.Format("Post with ID {0} not found. Can't undelete.", id));
			var user = this.CurrentUser();
			PostService.Undelete(post, user);
			return RedirectToAction("PostLink", "Forum", new {id = post.PostID});
		}

		public ViewResult TopicModerationLog(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				throw new Exception(String.Format("There is no topic with ID {0} to obtain a moderation log for.", id));
			var log = ModerationLogService.GetLog(topic, true);
			return View(log);
		}

		public ViewResult PostModerationLog(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				throw new Exception(String.Format("There is no post with ID {0} to obtain a moderation log for.", id));
			var log = ModerationLogService.GetLog(post);
			return View(log);
		}
	}
}
