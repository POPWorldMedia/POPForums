using System;
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
		Post PostReply(Topic topic, User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, string topicLink, Func<User, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator);
		void EditPost(Post post, PostEdit postEdit, User editingUser);
		BasicServiceResponse<Topic> PostNewTopic(User user, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator, Func<Topic, string> redirectLinkGenerator);
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

		public BasicServiceResponse<Topic> PostNewTopic(User user, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator, Func<Topic, string> redirectLinkGenerator)
		{
			if (user == null)
				return GetPostFailMessage(Resources.LoginToPost);
			var forum = _forumRepository.Get(newPost.ItemID);
			if (forum == null)
				throw new Exception($"Forum {newPost.ItemID} not found");
			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
				return GetPostFailMessage(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return GetPostFailMessage(Resources.ForumNoPost);
			newPost.FullText = newPost.IsPlainText ? _textParsingService.ForumCodeToHtml(newPost.FullText) : _textParsingService.ClientHtmlToHtml(newPost.FullText);
			if (IsNewPostDupeOrInTimeLimit(newPost.FullText, user))
				return GetPostFailMessage(string.Format(Resources.PostWait, _settingsManager.Current.MinimumSecondsBetweenPosts));
			if (string.IsNullOrWhiteSpace(newPost.FullText) || string.IsNullOrWhiteSpace(newPost.Title))
				return GetPostFailMessage(Resources.PostEmpty);
			newPost.Title = _textParsingService.Censor(newPost.Title);
			var urlName = newPost.Title.ToUniqueUrlName(_topicRepository.GetUrlNamesThatStartWith(newPost.Title.ToUrlName()));
			var timeStamp = DateTime.UtcNow;
			var topicID = _topicRepository.Create(forum.ForumID, newPost.Title, 0, 0, user.UserID, user.Name, user.UserID, user.Name, timeStamp, false, false, false, urlName);
			var postID = _postRepository.Create(topicID, 0, ip, true, newPost.IncludeSignature, user.UserID, user.Name, newPost.Title, newPost.FullText, timeStamp, false, user.Name, null, false, 0);
			_forumRepository.UpdateLastTimeAndUser(forum.ForumID, timeStamp, user.Name);
			_forumRepository.IncrementPostAndTopicCount(forum.ForumID);
			_profileRepository.SetLastPostID(user.UserID, postID);
			var topic = new Topic { TopicID = topicID, ForumID = forum.ForumID, IsClosed = false, IsDeleted = false, IsPinned = false, LastPostName = user.Name, LastPostTime = timeStamp, LastPostUserID = user.UserID, ReplyCount = 0, StartedByName = user.Name, StartedByUserID = user.UserID, Title = newPost.Title, UrlName = urlName, ViewCount = 0 };
			// <a href="{0}">{1}</a> started a new topic: <a href="{2}">{3}</a>
			var topicLink = topicLinkGenerator(topic);
			var message = string.Format(Resources.NewPostPublishMessage, userUrl, user.Name, topicLink, topic.Title);
			var forumHasViewRestrictions = _forumRepository.GetForumViewRoles(forum.ForumID).Count > 0;
			_eventPublisher.ProcessEvent(message, user, EventDefinitionService.StaticEventIDs.NewTopic, forumHasViewRestrictions);
			_eventPublisher.ProcessEvent(string.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true);
			forum = _forumRepository.Get(forum.ForumID);
			_broker.NotifyForumUpdate(forum);
			_broker.NotifyTopicUpdate(topic, forum, topicLink);
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
			_topicViewCountService.SetViewedTopic(topic);

			var redirectLink = redirectLinkGenerator(topic);

			return new BasicServiceResponse<Topic> {Data = topic, Message = null, Redirect = redirectLink, IsSuccessful = true};
		}

		private BasicServiceResponse<Topic> GetPostFailMessage(string message)
		{
			return new BasicServiceResponse<Topic> {Data = null, Message = message, Redirect = null, IsSuccessful = false};
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
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
			_profileRepository.SetLastPostID(user.UserID, postID);
			if (unsubscribeLinkGenerator != null)
				_subscribedTopicsService.NotifySubscribers(topic, user, topicLink, unsubscribeLinkGenerator);
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

		public void EditPost(Post post, PostEdit postEdit, User editingUser)
		{
			// TODO: text parsing is controller for new topic and replies, see issue #121 https://github.com/POPWorldMedia/POPForums/issues/121
			// TODO: also not checking for empty posts
			var oldText = post.FullText;
			post.Title = _textParsingService.Censor(postEdit.Title);
			if (postEdit.IsPlainText)
				post.FullText = _textParsingService.ForumCodeToHtml(postEdit.FullText);
			else
				post.FullText = _textParsingService.ClientHtmlToHtml(postEdit.FullText);
			post.ShowSig = postEdit.ShowSig;
			post.LastEditTime = DateTime.UtcNow;
			post.LastEditName = editingUser.Name;
			post.IsEdited = true;
			_postRepository.Update(post);
			_moderationLogService.LogPost(editingUser, ModerationType.PostEdit, post, postEdit.Comment, oldText);
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = post.TopicID });
		}

		private bool IsNewPostDupeOrInTimeLimit(string parsedPost, User user)
		{
			var postID = _profileRepository.GetLastPostID(user.UserID);
			if (postID == null)
				return false;
			var lastPost = _postRepository.Get(postID.Value);
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