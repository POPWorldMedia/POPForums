using System;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface IPostMasterService
	{
		Task<BasicServiceResponse<Post>> EditPost(int postID, PostEdit postEdit, User editingUser, Func<Post, string> redirectLinkGenerator);
		Task<BasicServiceResponse<Topic>> PostNewTopic(User user, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator, Func<Topic, string> redirectLinkGenerator);
		Task<BasicServiceResponse<Post>> PostReply(User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, Func<Topic, string> topicLinkGenerator, Func<User, Topic, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator, Func<Post, string> redirectLinkGenerator);
	}

	public class PostMasterService : IPostMasterService
	{
		private readonly ITextParsingService _textParsingService;
		private readonly ITopicRepository _topicRepository;
		private readonly IPostRepository _postRepository;
		private readonly IForumRepository _forumRepository;
		private readonly IProfileRepository _profileRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IBroker _broker;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;
		private readonly ISubscribedTopicsService _subscribedTopicsService;
		private readonly IModerationLogService _moderationLogService;
		private readonly IForumPermissionService _forumPermissionService;
		private readonly ISettingsManager _settingsManager;
		private readonly ITopicViewCountService _topicViewCountService;

		public PostMasterService(ITextParsingService textParsingService, ITopicRepository topicRepository, IPostRepository postRepository, IForumRepository forumRepository, IProfileRepository profileRepository, IEventPublisher eventPublisher, IBroker broker, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService, ISubscribedTopicsService subscribedTopicsService, IModerationLogService moderationLogService, IForumPermissionService forumPermissionService, ISettingsManager settingsManager, ITopicViewCountService topicViewCountService)
		{
			_textParsingService = textParsingService;
			_topicRepository = topicRepository;
			_postRepository = postRepository;
			_forumRepository = forumRepository;
			_profileRepository = profileRepository;
			_eventPublisher = eventPublisher;
			_broker = broker;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_tenantService = tenantService;
			_subscribedTopicsService = subscribedTopicsService;
			_moderationLogService = moderationLogService;
			_forumPermissionService = forumPermissionService;
			_settingsManager = settingsManager;
			_topicViewCountService = topicViewCountService;
		}

		public async Task<BasicServiceResponse<Topic>> PostNewTopic(User user, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator, Func<Topic, string> redirectLinkGenerator)
		{
			if (user == null)
				return GetPostFailMessage(Resources.LoginToPost);
			var forum = await _forumRepository.Get(newPost.ItemID);
			if (forum == null)
				throw new Exception($"Forum {newPost.ItemID} not found");
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
				return GetPostFailMessage(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return GetPostFailMessage(Resources.ForumNoPost);
			newPost.FullText = newPost.IsPlainText ? _textParsingService.ForumCodeToHtml(newPost.FullText) : _textParsingService.ClientHtmlToHtml(newPost.FullText);
			if (await IsNewPostDupeOrInTimeLimit(newPost.FullText, user))
				return GetPostFailMessage(string.Format(Resources.PostWait, _settingsManager.Current.MinimumSecondsBetweenPosts));
			if (string.IsNullOrWhiteSpace(newPost.FullText) || string.IsNullOrWhiteSpace(newPost.Title))
				return GetPostFailMessage(Resources.PostEmpty);
			newPost.Title = _textParsingService.Censor(newPost.Title);
			var urlName = newPost.Title.ToUniqueUrlName(await _topicRepository.GetUrlNamesThatStartWith(newPost.Title.ToUrlName()));
			var timeStamp = DateTime.UtcNow;
			var topicID = await _topicRepository.Create(forum.ForumID, newPost.Title, 0, 0, user.UserID, user.Name, user.UserID, user.Name, timeStamp, false, false, false, urlName);
			var postID = await _postRepository.Create(topicID, 0, ip, true, newPost.IncludeSignature, user.UserID, user.Name, newPost.Title, newPost.FullText, timeStamp, false, user.Name, null, false, 0);
			await _forumRepository.UpdateLastTimeAndUser(forum.ForumID, timeStamp, user.Name);
			await _forumRepository.IncrementPostAndTopicCount(forum.ForumID);
			await _profileRepository.SetLastPostID(user.UserID, postID);
			var topic = new Topic { TopicID = topicID, ForumID = forum.ForumID, IsClosed = false, IsDeleted = false, IsPinned = false, LastPostName = user.Name, LastPostTime = timeStamp, LastPostUserID = user.UserID, ReplyCount = 0, StartedByName = user.Name, StartedByUserID = user.UserID, Title = newPost.Title, UrlName = urlName, ViewCount = 0 };
			// <a href="{0}">{1}</a> started a new topic: <a href="{2}">{3}</a>
			var topicLink = topicLinkGenerator(topic);
			var message = string.Format(Resources.NewPostPublishMessage, userUrl, user.Name, topicLink, topic.Title);
			var forumHasViewRestrictions = _forumRepository.GetForumViewRoles(forum.ForumID).Result.Count > 0;
			await _eventPublisher.ProcessEvent(message, user, EventDefinitionService.StaticEventIDs.NewTopic, forumHasViewRestrictions);
			await _eventPublisher.ProcessEvent(string.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true);
			forum = await _forumRepository.Get(forum.ForumID);
			_broker.NotifyForumUpdate(forum);
			_broker.NotifyTopicUpdate(topic, forum, topicLink);
			await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID, IsForRemoval = false });
			_topicViewCountService.SetViewedTopic(topic);

			var redirectLink = redirectLinkGenerator(topic);

			return new BasicServiceResponse<Topic> {Data = topic, Message = null, Redirect = redirectLink, IsSuccessful = true};
		}

		private BasicServiceResponse<Topic> GetPostFailMessage(string message)
		{
			return new BasicServiceResponse<Topic> {Data = null, Message = message, Redirect = null, IsSuccessful = false};
		}

		private BasicServiceResponse<Post> GetReplyFailMessage(string message)
		{
			return new BasicServiceResponse<Post> { Data = null, Message = message, Redirect = null, IsSuccessful = false };
		}

		public async Task<BasicServiceResponse<Post>> PostReply(User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, Func<Topic, string> topicLinkGenerator, Func<User, Topic, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator, Func<Post, string> redirectLinkGenerator)
		{
			if (user == null)
				return GetReplyFailMessage(Resources.LoginToPost);
			var topic = await _topicRepository.Get(newPost.ItemID);
			if (topic == null)
				return GetReplyFailMessage(Resources.TopicNotExist);
			if (topic.IsClosed)
				return GetReplyFailMessage(Resources.Closed);
			var forum = await _forumRepository.Get(topic.ForumID);
			if (forum == null)
				throw new Exception($"That's not good. Trying to reply to a topic orphaned from Forum {topic.ForumID}, which doesn't exist.");
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
				return GetReplyFailMessage(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return GetReplyFailMessage(Resources.ForumNoPost);
			newPost.FullText = newPost.IsPlainText ? _textParsingService.ForumCodeToHtml(newPost.FullText) : _textParsingService.ClientHtmlToHtml(newPost.FullText);
			if (await IsNewPostDupeOrInTimeLimit(newPost.FullText, user))
				return GetReplyFailMessage(string.Format(Resources.PostWait, _settingsManager.Current.MinimumSecondsBetweenPosts));
			if (string.IsNullOrEmpty(newPost.FullText))
				return GetReplyFailMessage(Resources.PostEmpty);
			if (newPost.ParentPostID != 0)
			{
				var parentPost = await _postRepository.Get(newPost.ParentPostID);
				if (parentPost == null || parentPost.TopicID != topic.TopicID)
					return GetReplyFailMessage("This reply attempt is being made to a post in another topic");
			}
			newPost.Title = _textParsingService.Censor(newPost.Title);

			var postID = await _postRepository.Create(topic.TopicID, parentPostID, ip, isFirstInTopic, newPost.IncludeSignature, user.UserID, user.Name, newPost.Title, newPost.FullText, postTime, false, user.Name, null, false, 0);
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
			await _topicRepository.IncrementReplyCount(topic.TopicID);
			await _topicRepository.UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime);
			await _forumRepository.UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name);
			await _forumRepository.IncrementPostCount(topic.ForumID);
			await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
			await _profileRepository.SetLastPostID(user.UserID, postID);
			var topicLink = topicLinkGenerator(topic);
			if (unsubscribeLinkGenerator != null)
				await _subscribedTopicsService.NotifySubscribers(topic, user, topicLink, unsubscribeLinkGenerator);
			// <a href="{0}">{1}</a> made a post in the topic: <a href="{2}">{3}</a>
			var message = string.Format(Resources.NewReplyPublishMessage, userUrl, user.Name, postLinkGenerator(post), topic.Title);
			var forumHasViewRestrictions = _forumRepository.GetForumViewRoles(topic.ForumID).Result.Count > 0;
			await _eventPublisher.ProcessEvent(message, user, EventDefinitionService.StaticEventIDs.NewPost, forumHasViewRestrictions);
			topic = await _topicRepository.Get(topic.TopicID);
			forum = await _forumRepository.Get(forum.ForumID);
			_broker.NotifyNewPosts(topic, post.PostID);
			_broker.NotifyNewPost(topic, post.PostID);
			_broker.NotifyForumUpdate(forum);
			_broker.NotifyTopicUpdate(topic, forum, topicLink);
			_topicViewCountService.SetViewedTopic(topic);
			if (newPost.CloseOnReply && user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogTopic(user, ModerationType.TopicClose, topic, null);
				await _topicRepository.CloseTopic(topic.TopicID);
			}
			var redirectLink = redirectLinkGenerator(post);

			return new BasicServiceResponse<Post> { Data = post, Message = null, Redirect = redirectLink, IsSuccessful = true };
		}

		public async Task<BasicServiceResponse<Post>> EditPost(int postID, PostEdit postEdit, User editingUser, Func<Post, string> redirectLinkGenerator)
		{
			var censoredNewTitle = _textParsingService.Censor(postEdit.Title);
			var post = await _postRepository.Get(postID);
			if (!editingUser.IsPostEditable(post))
				return GetReplyFailMessage(Resources.Forbidden);
			var oldText = post.FullText;
			if (post.IsFirstInTopic && post.Title != censoredNewTitle)
			{
				if (string.IsNullOrEmpty(censoredNewTitle))
					return GetReplyFailMessage(Resources.PostEmpty);
				var oldTitle = post.Title;
				post.Title = censoredNewTitle;
				var topic = await _topicRepository.Get(post.TopicID);
				var forum = await _forumRepository.Get(topic.ForumID);
				var urlName = censoredNewTitle.ToUniqueUrlName(await _topicRepository.GetUrlNamesThatStartWith(censoredNewTitle.ToUrlName()));
				await _topicRepository.UpdateTitleAndForum(topic.TopicID, forum.ForumID, censoredNewTitle, urlName);
				await _moderationLogService.LogTopic(editingUser, ModerationType.TopicRenamed, topic, forum, $"Old title: {oldTitle}");
			}
			if (postEdit.IsPlainText)
				post.FullText = _textParsingService.ForumCodeToHtml(postEdit.FullText);
			else
				post.FullText = _textParsingService.ClientHtmlToHtml(postEdit.FullText);
			if (string.IsNullOrEmpty(postEdit.FullText))
				return GetReplyFailMessage(Resources.PostEmpty);
			post.ShowSig = postEdit.ShowSig;
			post.LastEditTime = DateTime.UtcNow;
			post.LastEditName = editingUser.Name;
			post.IsEdited = true;
			await _postRepository.Update(post);
			await _moderationLogService.LogPost(editingUser, ModerationType.PostEdit, post, postEdit.Comment, oldText);
			await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = post.TopicID, IsForRemoval = false });
			var redirectLink = redirectLinkGenerator(post);
			return new BasicServiceResponse<Post> { Data = post, IsSuccessful = true, Message = string.Empty, Redirect = redirectLink };
		}

		private async Task<bool> IsNewPostDupeOrInTimeLimit(string parsedPost, User user)
		{
			var postID = await _profileRepository.GetLastPostID(user.UserID);
			if (postID == null)
				return false;
			var lastPost = await _postRepository.Get(postID.Value);
			if (lastPost == null)
				return false;
			var minimumSeconds = _settingsManager.Current.MinimumSecondsBetweenPosts;
			if (DateTime.UtcNow.Subtract(lastPost.PostTime).TotalSeconds < minimumSeconds)
				return true;
			if (parsedPost == lastPost.FullText)
				return true;
			return false;
		}
	}
}