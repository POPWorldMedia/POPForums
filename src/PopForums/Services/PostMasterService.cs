using System;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface IPostMasterService
	{
		Topic PostNewTopic(Forum forum, User user, ForumPermissionContext permissionContext, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator);
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

		public PostMasterService(ITextParsingService textParsingService, ITopicRepository topicRepository, IPostRepository postRepository, IForumRepository forumRepository, IProfileRepository profileRepository, IEventPublisher eventPublisher, IBroker broker, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
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
		}

		public Topic PostNewTopic(Forum forum, User user, ForumPermissionContext permissionContext, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator)
		{
			if (!permissionContext.UserCanPost || !permissionContext.UserCanView)
				throw new Exception($"User {user.Name} can't post to forum {forum.Title}.");
			newPost.Title = _textParsingService.Censor(newPost.Title);
			// TODO: text parsing is controller, see issue #121 https://github.com/POPWorldMedia/POPForums/issues/121
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
			var message = String.Format(Resources.NewPostPublishMessage, userUrl, user.Name, topicLink, topic.Title);
			var forumHasViewRestrictions = _forumRepository.GetForumViewRoles(forum.ForumID).Count > 0;
			_eventPublisher.ProcessEvent(message, user, EventDefinitionService.StaticEventIDs.NewTopic, forumHasViewRestrictions);
			_eventPublisher.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true);
			forum = _forumRepository.Get(forum.ForumID);
			_broker.NotifyForumUpdate(forum);
			_broker.NotifyTopicUpdate(topic, forum, topicLink);
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
			return topic;
		}
	}
}