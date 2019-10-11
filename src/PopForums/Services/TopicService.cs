using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface ITopicService
	{
		Task<Tuple<List<Topic>, PagerContext>> GetTopics(Forum forum, bool includeDeleted, int pageIndex);
		Task<Topic> Get(string urlName);
		Task<Topic> Get(int topicID);
		Task CloseTopic(Topic topic, User user);
		Task OpenTopic(Topic topic, User user);
		Task PinTopic(Topic topic, User user);
		Task UnpinTopic(Topic topic, User user);
		Task DeleteTopic(Topic topic, User user);
		Task UndeleteTopic(Topic topic, User user);
		Task UpdateTitleAndForum(Topic topic, Forum forum, string newTitle, User user);
		Task<Tuple<List<Topic>, PagerContext>> GetTopics(User viewingUser, User postUser, bool includeDeleted, int pageIndex);
		Task RecalculateReplyCount(Topic topic);
		Task<List<Topic>> GetTopics(User viewingUser, Forum forum, bool includeDeleted);
		Task UpdateLast(Topic topic);
		Task<int> TopicLastPostID(int topicID);
		Task HardDeleteTopic(Topic topic, User user);
		Task SetAnswer(User user, Topic topic, Post post, string userUrl, string topicUrl);
		Task QueueTopicForIndexing(int topicID);
		Task CloseAgedTopics();
	}

	public class TopicService : ITopicService
	{
		public TopicService(ITopicRepository topicRepository, IPostRepository postRepository, ISettingsManager settingsManager, IModerationLogService moderationLogService, IForumService forumService, IEventPublisher eventPublisher, ISearchRepository searchRepository, IUserRepository userRepository, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
		{
			_topicRepository = topicRepository;
			_postRepository = postRepository;
			_settingsManager = settingsManager;
			_moderationLogService = moderationLogService;
			_forumService = forumService;
			_eventPublisher = eventPublisher;
			_searchRepository = searchRepository;
			_userRepository = userRepository;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_tenantService = tenantService;
		}

		private readonly ITopicRepository _topicRepository;
		private readonly IPostRepository _postRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly IModerationLogService _moderationLogService;
		private readonly IForumService _forumService;
		private readonly IEventPublisher _eventPublisher;
		private readonly ISearchRepository _searchRepository;
		private readonly IUserRepository _userRepository;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;

		public async Task<Tuple<List<Topic>, PagerContext>> GetTopics(Forum forum, bool includeDeleted, int pageIndex)
		{
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = await _topicRepository.Get(forum.ForumID, includeDeleted, startRow, pageSize);
			int topicCount;
			if (includeDeleted)
				topicCount = await _topicRepository.GetTopicCount(forum.ForumID, true);
			else
				topicCount = forum.TopicCount;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return Tuple.Create(topics, pagerContext);
		}

		public async Task<List<Topic>> GetTopics(User viewingUser, Forum forum, bool includeDeleted)
		{
			var nonViewableForumIDs = await _forumService.GetNonViewableForumIDs(viewingUser);
			var topics = await _topicRepository.Get(forum.ForumID, includeDeleted, nonViewableForumIDs);
			return topics;
		}

		public async Task<Tuple<List<Topic>, PagerContext>> GetTopics(User viewingUser, User postUser, bool includeDeleted, int pageIndex)
		{
			var nonViewableForumIDs = await _forumService.GetNonViewableForumIDs(viewingUser);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = await _topicRepository.GetTopicsByUser(postUser.UserID, includeDeleted, nonViewableForumIDs, startRow, pageSize);
			var topicCount = await _topicRepository.GetTopicCountByUser(postUser.UserID, includeDeleted, nonViewableForumIDs);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return Tuple.Create(topics, pagerContext);
		}

		public async Task<Topic> Get(string urlName)
		{
			return await _topicRepository.Get(urlName);
		}

		public async Task<Topic> Get(int topicID)
		{
			return await _topicRepository.Get(topicID);
		}

		public async Task CloseTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicClose, topic, null);
				await _topicRepository.CloseTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to close topic.");
		}

		public async Task OpenTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicOpen, topic, null);
				await _topicRepository.OpenTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to open topic.");
		}

		public async Task PinTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicPin, topic, null);
				await _topicRepository.PinTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to pin topic.");
		}

		public async Task UnpinTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicUnpin, topic, null);
				await _topicRepository.UnpinTopic(topic.TopicID);
			}
			else
				throw new InvalidOperationException("User must be Moderator to unpin topic.");
		}

		public async Task DeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator) || user.UserID == topic.StartedByUserID)
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicDelete, topic, null);
				await _topicRepository.DeleteTopic(topic.TopicID);
				await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID, IsForRemoval = true });
				await RecalculateReplyCount(topic);
				var forum = await _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				await _forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Moderator or topic starter to delete topic.");
		}

		public async Task HardDeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Admin))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicDeletePermanently, topic, null);
				await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID, IsForRemoval = true });
				await _topicRepository.HardDeleteTopic(topic.TopicID);
				var forum = await _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				await _forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Admin to hard delete topic.");
		}

		public async Task UndeleteTopic(Topic topic, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicUndelete, topic, null);
				await _topicRepository.UndeleteTopic(topic.TopicID);
				await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID, IsForRemoval = false });
				await RecalculateReplyCount(topic);
				var forum = await _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				await _forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Moderator to undelete topic.");
		}

		public async Task UpdateTitleAndForum(Topic topic, Forum forum, string newTitle, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				var oldTopic = await _topicRepository.Get(topic.TopicID);
				if (oldTopic.ForumID != forum.ForumID)
					await _moderationLogService.LogTopic(user, ModerationType.TopicMoved, topic, forum, $"Moved from {oldTopic.ForumID} to {forum.ForumID}");
				if (oldTopic.Title != newTitle)
					await _moderationLogService.LogTopic(user, ModerationType.TopicRenamed, topic, forum, $"Renamed from \"{oldTopic.Title}\" to \"{newTitle}\"");
				var urlName = newTitle.ToUniqueUrlName(await _topicRepository.GetUrlNamesThatStartWith(newTitle.ToUrlName()));
				topic.UrlName = urlName;
				await _topicRepository.UpdateTitleAndForum(topic.TopicID, forum.ForumID, newTitle, urlName);
				await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID, IsForRemoval = false });
				_forumService.UpdateCounts(forum);
				await _forumService.UpdateLast(forum);
				var oldForum = await _forumService.Get(oldTopic.ForumID);
				_forumService.UpdateCounts(oldForum);
				await _forumService.UpdateLast(oldForum);
			}
			else
				throw new InvalidOperationException("User must be Moderator to update topic title or move topic.");
		}

		public async Task RecalculateReplyCount(Topic topic)
		{
			var replyCount = await _postRepository.GetReplyCount(topic.TopicID, false);
			await _topicRepository.UpdateReplyCount(topic.TopicID, replyCount);
		}

		public async Task UpdateLast(Topic topic)
		{
			var post = await _postRepository.GetLastInTopic(topic.TopicID);
			await _topicRepository.UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime);
		}

		public async Task<int> TopicLastPostID(int topicID)
		{
			var post = await _postRepository.GetLastInTopic(topicID);
			if (post == null)
				return 0;
			return post.PostID;
		}

		public async Task SetAnswer(User user, Topic topic, Post post, string userUrl, string topicUrl)
		{
			if (user.UserID != topic.StartedByUserID)
				throw new SecurityException("Only the user that started a topic may set its answer.");
			if (post == null || post.TopicID != topic.TopicID)
				throw new InvalidOperationException("You can't use a post as an answer unless it's a child of the topic.");
			var answerUser = await _userRepository.GetUser(post.UserID);
			if (answerUser != null // answer user is still valid
				&& !topic.AnswerPostID.HasValue && // an answer wasn't already chosen
				topic.StartedByUserID != post.UserID) // the answer isn't coming from the question asker
			{
				// <a href="{0}">{1}</a> chose an answer for the question: <a href="{2}">{3}</a>
				var message = String.Format(Resources.QuestionAnswered, userUrl, user.Name, topicUrl, topic.Title);
				await _eventPublisher.ProcessEvent(message, answerUser, EventDefinitionService.StaticEventIDs.QuestionAnswered, false);
			}
			await _topicRepository.UpdateAnswerPostID(topic.TopicID, post.PostID);
		}

		public async Task QueueTopicForIndexing(int topicID)
		{
			await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topicID, IsForRemoval = false });
		}

		public async Task CloseAgedTopics()
		{
			if (!_settingsManager.Current.IsClosingAgedTopics)
				return;
			var ageCutoff = DateTime.UtcNow.AddDays(-_settingsManager.Current.CloseAgedTopicsDays);
			var list = await _topicRepository.CloseTopicsOlderThan(ageCutoff);
			foreach (var id in list)
				await _moderationLogService.LogTopic(ModerationType.TopicCloseAuto, id);
		}
	}
}