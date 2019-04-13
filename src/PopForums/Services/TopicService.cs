using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface ITopicService
	{
		List<Topic> GetTopics(Forum forum, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		Post PostReply(Topic topic, User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, string topicLink, Func<User, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator);
		Topic Get(string urlName);
		Topic Get(int topicID);
		void CloseTopic(Topic topic, User user);
		void OpenTopic(Topic topic, User user);
		void PinTopic(Topic topic, User user);
		void UnpinTopic(Topic topic, User user);
		void DeleteTopic(Topic topic, User user);
		void UndeleteTopic(Topic topic, User user);
		void UpdateTitleAndForum(Topic topic, Forum forum, string newTitle, User user);
		List<Topic> GetTopics(User viewingUser, User postUser, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		void RecalculateReplyCount(Topic topic);
		Dictionary<int, int> GetFirstPostIDsFromTopics(List<Topic> topics);
		DateTime? TopicLastPostTime(int topicID);
		List<Topic> GetTopics(User viewingUser, Forum forum, bool includeDeleted);
		void UpdateLast(Topic topic);
		int TopicLastPostID(int topicID);
		void HardDeleteTopic(Topic topic, User user);
		void SetAnswer(User user, Topic topic, Post post, string userUrl, string topicUrl);
		void QueueTopicForIndexing(int topicID);
	}

	public class TopicService : ITopicService
	{
		public TopicService(IForumRepository forumRepository, ITopicRepository topicRepository, IPostRepository postRepository, IProfileRepository profileRepository, ITextParsingService textParsingService, ISettingsManager settingsManager, ISubscribedTopicsService subscribedTopicsService, IModerationLogService moderationLogService, IForumService forumService, IEventPublisher eventPublisher, IBroker broker, ISearchRepository searchRepository, IUserRepository userRepository, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
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
			_searchRepository = searchRepository;
			_userRepository = userRepository;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_tenantService = tenantService;
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
		private readonly ISearchRepository _searchRepository;
		private readonly IUserRepository _userRepository;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;

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
			newPost.Title = _textParsingService.Censor(newPost.Title);
			// TODO: text parsing is controller, see issue #121 https://github.com/POPWorldMedia/POPForums/issues/121
			var postID = _postRepository.Create(topic.TopicID, parentPostID, ip, isFirstInTopic, newPost.IncludeSignature, user.UserID, user.Name, newPost.Title, newPost.FullText, postTime, false, user.Name, null, false, 0);
			var post = new Post
			{
				PostID = postID,
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
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload {TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID});
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

		public void HardDeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Admin))
			{
				_moderationLogService.LogTopic(user, ModerationType.TopicDeletePermanently, topic, null);
				_searchRepository.DeleteAllIndexedWordsForTopic(topic.TopicID);
				_topicRepository.HardDeleteTopic(topic.TopicID);
				var forum = _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				_forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Admin to hard delete topic.");
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
				_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
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

		public void SetAnswer(User user, Topic topic, Post post, string userUrl, string topicUrl)
		{
			if (user.UserID != topic.StartedByUserID)
				throw new SecurityException("Only the user that started a topic may set its answer.");
			if (post == null || post.TopicID != topic.TopicID)
				throw new InvalidOperationException("You can't use a post as an answer unless it's a child of the topic.");
			var answerUser = _userRepository.GetUser(post.UserID);
			if (answerUser != null // answer user is still valid
				&& !topic.AnswerPostID.HasValue && // an answer wasn't already chosen
				topic.StartedByUserID != post.UserID) // the answer isn't coming from the question asker
			{
				// <a href="{0}">{1}</a> chose an answer for the question: <a href="{2}">{3}</a>
				var message = String.Format(Resources.QuestionAnswered, userUrl, user.Name, topicUrl, topic.Title);
				_eventPublisher.ProcessEvent(message, answerUser, EventDefinitionService.StaticEventIDs.QuestionAnswered, false);
			}
			_topicRepository.UpdateAnswerPostID(topic.TopicID, post.PostID);
		}

		public void QueueTopicForIndexing(int topicID)
		{
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topicID });
		}
	}
}