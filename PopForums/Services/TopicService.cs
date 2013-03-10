using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public class TopicService : ITopicService
	{
		public TopicService(IForumRepository forumRepository, ITopicRepository topicRepository, IPostRepository postRepository, IProfileRepository profileRepository, ITextParsingService textParsingService, ISettingsManager settingsManager, ISubscribedTopicsService subscribedTopicsService, IModerationLogService moderationLogService, IForumService forumService, IEventPublisher eventPublisher, IBroker broker)
		{
			_forumRepository = forumRepository;
			_topicRepository = topicRepository;
			_postRepository = postRepository;
			_profileRepository = profileRepository;
			_settingsManager = settingsManager;
			_textParsingService = textParsingService;
			_subscribedTopicService = subscribedTopicsService;
			_moderationLogService = moderationLogService;
			_forumService = forumService;
			_eventPublisher = eventPublisher;
			_broker = broker;
		}

		private readonly IForumRepository _forumRepository;
		private readonly ITopicRepository _topicRepository;
		private readonly IPostRepository _postRepository;
		private readonly IProfileRepository _profileRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly ITextParsingService _textParsingService;
		private readonly ISubscribedTopicsService _subscribedTopicService;
		private readonly IModerationLogService _moderationLogService;
		private readonly IForumService _forumService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IBroker _broker;

		public List<Topic> GetTopics(Forum forum, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = _topicRepository.Get(forum.ForumID, includeDeleted, startRow, pageSize);
			int topicCount;
			if (includeDeleted)
				topicCount = _topicRepository.GetTopicCount(forum.ForumID, true);
			else
				topicCount = forum.TopicCount;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public List<Topic> GetTopics(User viewingUser, Forum forum, bool includeDeleted)
		{
			var nonViewableForumIDs = _forumService.GetNonViewableForumIDs(viewingUser);
			var topics = _topicRepository.Get(forum.ForumID, includeDeleted, nonViewableForumIDs);
			return topics;
		}

		public List<Topic> GetTopics(User viewingUser, User postUser, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var nonViewableForumIDs = _forumService.GetNonViewableForumIDs(viewingUser);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = _topicRepository.GetTopicsByUser(postUser.UserID, includeDeleted, nonViewableForumIDs, startRow, pageSize);
			var topicCount = _topicRepository.GetTopicCountByUser(postUser.UserID, includeDeleted, nonViewableForumIDs);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public Dictionary<int, int> GetFirstPostIDsFromTopics(List<Topic> topics)
		{
			return _postRepository.GetFirstPostIDsFromTopicIDs(topics.Select(x => x.TopicID).ToList());
		}

		public Topic Get(string urlName)
		{
			return _topicRepository.Get(urlName);
		}

		public Topic Get(int topicID)
		{
			return _topicRepository.Get(topicID);
		}

		public Post PostReply(Topic topic, User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, string topicLink, Func<User, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator)
		{
			newPost.Title = _textParsingService.EscapeHtmlAndCensor(newPost.Title);
			if (newPost.IsPlainText)
				newPost.FullText = _textParsingService.ForumCodeToHtml(newPost.FullText);
			else
				newPost.FullText = _textParsingService.ClientHtmlToHtml(newPost.FullText);
			var postID = _postRepository.Create(topic.TopicID, parentPostID, ip, isFirstInTopic, newPost.IncludeSignature, user.UserID, user.Name, newPost.Title, newPost.FullText, postTime, false, user.Name, null, false, 0);
			var post = new Post(postID)
			{
				FullText = newPost.FullText,
				IP = ip,
				IsDeleted = false,
				IsEdited = false,
				IsFirstInTopic = isFirstInTopic,
				LastEditName = user.Name,
				LastEditTime = null,
				Name = user.Name,
				ParentPostID = parentPostID,
				PostTime = postTime,
				ShowSig = newPost.IncludeSignature,
				Title = newPost.Title,
				TopicID = topic.TopicID,
				UserID = user.UserID
			};
			_topicRepository.IncrementReplyCount(topic.TopicID);
			_topicRepository.UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime);
			_forumRepository.UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name);
			_forumRepository.IncrementPostCount(topic.ForumID);
			_profileRepository.SetLastPostID(user.UserID, postID);
			if (unsubscribeLinkGenerator != null)
				_subscribedTopicService.NotifySubscribers(topic, user, topicLink, unsubscribeLinkGenerator);
			// <a href="{0}">{1}</a> made a post in the topic: <a href="{2}">{3}</a>
			var message = String.Format(Resources.NewReplyPublishMessage, userUrl, user.Name, postLinkGenerator(post), topic.Title);
			var forumHasViewRestrictions = _forumRepository.GetForumViewRoles(topic.ForumID).Count > 0;
			_eventPublisher.ProcessEvent(message, user, EventDefinitionService.StaticEventIDs.NewPost, forumHasViewRestrictions);
			_broker.NotifyNewPosts(topic, post.PostID);
			_broker.NotifyNewPost(topic, post.PostID);
			var forum = _forumRepository.Get(topic.ForumID);
			_broker.NotifyForumUpdate(forum);
			topic = _topicRepository.Get(topic.TopicID);
			_broker.NotifyTopicUpdate(topic, forum, topicLink);
			return post;
		}

		public void CloseTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicClose, topic, null);
				_topicRepository.CloseTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to close topic.");
		}

		public void OpenTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicOpen, topic, null);
				_topicRepository.OpenTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to open topic.");
		}

		public void PinTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicPin, topic, null);
				_topicRepository.PinTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to pin topic.");
		}

		public void UnpinTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicUnpin, topic, null);
				_topicRepository.UnpinTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to unpin topic.");
		}

		public void DeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator) || user.UserID == topic.StartedByUserID)
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicDelete, topic, null);
				_topicRepository.DeleteTopic(topic.TopicID);
				RecalculateReplyCount(topic);
				var forum = _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				_forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Moderator or topic starter to delete topic.");
		}

		public void UndeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicUndelete, topic, null);
				_topicRepository.UndeleteTopic(topic.TopicID);
				RecalculateReplyCount(topic);
				var forum = _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				_forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Moderator to undelete topic.");
		}

		public void UpdateTitleAndForum(Topic topic, Forum forum, string newTitle, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				var oldTopic = _topicRepository.Get(topic.TopicID);
				if (oldTopic.ForumID != forum.ForumID)
					_moderationLogService.LogTopic(user, ModerationType.TopicMoved, topic, forum, String.Format("Moved from {0} to {1}", oldTopic.ForumID, forum.ForumID));
				if (oldTopic.Title != newTitle)
					_moderationLogService.LogTopic(user, ModerationType.TopicRenamed, topic, forum, String.Format("Renamed from \"{0}\" to \"{1}\"", oldTopic.Title, newTitle));
				var urlName = newTitle.ToUniqueUrlName(_topicRepository.GetUrlNamesThatStartWith(newTitle.ToUrlName()));
				topic.UrlName = urlName;
				_topicRepository.UpdateTitleAndForum(topic.TopicID, forum.ForumID, newTitle, urlName);
				_forumService.UpdateCounts(forum);
				_forumService.UpdateLast(forum);
				var oldForum = _forumService.Get(oldTopic.ForumID);
				_forumService.UpdateCounts(oldForum);
				_forumService.UpdateLast(oldForum);
			}
			else
				throw new InvalidOperationException("User must be Moderator to update topic title or move topic.");
		}

		public void RecalculateReplyCount(Topic topic)
		{
			var replyCount = _postRepository.GetReplyCount(topic.TopicID, false);
			_topicRepository.UpdateReplyCount(topic.TopicID, replyCount);
		}

		public DateTime? TopicLastPostTime(int topicID)
		{
			return _topicRepository.GetLastPostTime(topicID);
		}

		public void UpdateLast(Topic topic)
		{
			var post = _postRepository.GetLastInTopic(topic.TopicID);
			_topicRepository.UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime);
		}

		public int TopicLastPostID(int topicID)
		{
			var post = _postRepository.GetLastInTopic(topicID);
			if (post == null)
				return 0;
			return post.PostID;
		}
	}
}