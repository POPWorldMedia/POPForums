using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface IForumService
	{
		Forum Get(int forumID);
		Forum Get(string urlName);
		Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName, bool isQAForum);
		void UpdateLast(Forum forum);
		void UpdateLast(Forum forum, DateTime lastTime, string lastName);
		void UpdateCounts(Forum forum);
		void IncrementPostCount(Forum forum);
		void IncrementPostAndTopicCount(Forum forum);
		CategorizedForumContainer GetCategorizedForumContainer();
		List<CategoryContainerWithForums> GetCategoryContainersWithForums();
		CategorizedForumContainer GetCategorizedForumContainerFilteredForUser(User user);
		List<int> GetNonViewableForumIDs(User user);
		ForumPermissionContext GetPermissionContext(Forum forum, User user);
		ForumPermissionContext GetPermissionContext(Forum forum, User user, Topic topic);
		Topic PostNewTopic(Forum forum, User user, ForumPermissionContext permissionContext, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator);
		void Update(Forum forum, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum);
		void MoveForumUp(Forum forum);
		void MoveForumUp(int forumID);
		void MoveForumDown(Forum forum);
		void MoveForumDown(int forumID);
		List<string> GetForumPostRoles(Forum forum);
		List<string> GetForumViewRoles(Forum forum);
		void AddPostRole(Forum forum, string role);
		void RemovePostRole(Forum forum, string role);
		void AddViewRole(Forum forum, string role);
		void RemoveViewRole(Forum forum, string role);
		void RemoveAllPostRoles(Forum forum);
		void RemoveAllViewRoles(Forum forum);
		Dictionary<int, string> GetAllForumTitles();
		List<Topic> GetRecentTopics(User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		int GetAggregateTopicCount();
		int GetAggregatePostCount();
		List<int> GetViewableForumIDsFromViewRestrictedForums(User user);
		TopicContainerForQA MapTopicContainerForQA(TopicContainer topicContainer);
		void ModifyForumRoles(ModifyForumRolesContainer container);
	}

	public class ForumService : IForumService
	{
		public ForumService(IForumRepository forumRepository, ITopicRepository topicRepository, IPostRepository postRepository, ICategoryRepository categoryRepository, IProfileRepository profileRepository, ITextParsingService textParsingService, ISettingsManager settingsManager, ILastReadService lastReadService, IEventPublisher eventPublisher, IBroker broker, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
		{
			_forumRepository = forumRepository;
			_topicRepository = topicRepository;
			_postRepository = postRepository;
			_categoryRepository = categoryRepository;
			_profileRepository = profileRepository;
			_settingsManager = settingsManager;
			_textParsingService = textParsingService;
			_lastReadService = lastReadService;
			_eventPublisher = eventPublisher;
			_broker = broker;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_tenantService = tenantService;
		}

		private readonly IForumRepository _forumRepository;
		private readonly ITopicRepository _topicRepository;
		private readonly IPostRepository _postRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IProfileRepository _profileRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly ITextParsingService _textParsingService;
		private readonly ILastReadService _lastReadService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IBroker _broker;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;

		public Forum Get(int forumID)
		{
			return _forumRepository.Get(forumID);
		}

		public Forum Get(string urlName)
		{
			return _forumRepository.Get(urlName);
		}

		public Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName, bool isQAForum)
		{
			var urlName = title.ToUniqueUrlName(_forumRepository.GetUrlNamesThatStartWith(title.ToUrlName()));
			var forum = _forumRepository.Create(categoryID, title, description, isVisible, isArchived, sortOrder, urlName, forumAdapterName, isQAForum);
			forum.UrlName = urlName;
			var forums = _forumRepository.GetAll().ToList();
			SortAndUpdateForums(forums);
			return forum;
		}

		public void Update(Forum forum, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
		{
			var urlName = forum.UrlName;
			if (forum.Title != title)
				urlName = title.ToUniqueUrlName(_forumRepository.GetUrlNamesThatStartWith(title.ToUrlName()));
			_forumRepository.Update(forum.ForumID, categoryID, title, description, isVisible, isArchived, urlName, forumAdapterName, isQAForum);
		}

		public void UpdateLast(Forum forum)
		{
			var topic = _topicRepository.GetLastUpdatedTopic(forum.ForumID);
			if (topic != null)
				UpdateLast(forum, topic.LastPostTime, topic.LastPostName);
			else
				UpdateLast(forum, new DateTime(2000, 1, 1), String.Empty);
		}

		public void UpdateLast(Forum forum, DateTime lastTime, string lastName)
		{
			_forumRepository.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName);
		}

		public void UpdateCounts(Forum forum)
		{
			new Thread(() =>
			{
				var topicCount = _topicRepository.GetTopicCount(forum.ForumID, false);
				var postCount = _topicRepository.GetPostCount(forum.ForumID, false);
				_forumRepository.UpdateTopicAndPostCounts(forum.ForumID, topicCount, postCount);
			}).Start();
		}

		public void IncrementPostCount(Forum forum)
		{
			_forumRepository.IncrementPostCount(forum.ForumID);
		}

		public void IncrementPostAndTopicCount(Forum forum)
		{
			_forumRepository.IncrementPostAndTopicCount(forum.ForumID);
		}

		public CategorizedForumContainer GetCategorizedForumContainer()
		{
			var forums = _forumRepository.GetAll();
			var categories = _categoryRepository.GetAll();
			var container = new CategorizedForumContainer(categories, forums);
			container.ForumTitle = _settingsManager.Current.ForumTitle;
			return container;
		}

		public List<CategoryContainerWithForums> GetCategoryContainersWithForums()
		{
			var containers = new List<CategoryContainerWithForums>();
			var forums = _forumRepository.GetAll().ToList();
			var categories = _categoryRepository.GetAll().OrderBy(x => x.SortOrder);
			var uncategorized = forums.Where(x => !x.CategoryID.HasValue).OrderBy(x => x.SortOrder).ToList();
			if (uncategorized.Count > 0)
				containers.Add(new CategoryContainerWithForums {Category = new Category { Title = Resources.ForumsUncat }, Forums = uncategorized});
			foreach (var item in categories)
			{
				var filteredForums = forums.Where(x => x.CategoryID == item.CategoryID).OrderBy(x => x.SortOrder);
				containers.Add(new CategoryContainerWithForums { Category = item, Forums = filteredForums });
			}
			return containers;
		}

		public List<int> GetViewableForumIDsFromViewRestrictedForums(User user)
		{
			var nonViewableForumIDs = GetNonViewableForumIDs(user);
			var noViewRestrictionForums = _forumRepository.GetAllVisible().Where(f => !nonViewableForumIDs.Contains(f.ForumID));
			return noViewRestrictionForums.Select(x => x.ForumID).ToList();
		}

		public CategorizedForumContainer GetCategorizedForumContainerFilteredForUser(User user)
		{
			var nonViewableForumIDs = GetNonViewableForumIDs(user);
			var forums = _forumRepository.GetAllVisible().Where(f => !nonViewableForumIDs.Contains(f.ForumID));
			var categories = _categoryRepository.GetAll();
			var container = new CategorizedForumContainer(categories, forums);
			_lastReadService.GetForumReadStatus(user, container);
			container.ForumTitle = _settingsManager.Current.ForumTitle;
			return container;
		}

		public List<int> GetNonViewableForumIDs(User user)
		{
			var forumsWithRestrictions = _forumRepository.GetForumViewRestrictionRoleGraph();
			var nonViewableForums = new List<int>();
			foreach (var item in forumsWithRestrictions.Where(f => f.Value.Count > 0))
			{
				if (user == null)
					nonViewableForums.Add(item.Key);
				else if (user.Roles.Intersect(item.Value).Count() == 0)
					nonViewableForums.Add(item.Key);
			}
			return nonViewableForums;
		}

		public ForumPermissionContext GetPermissionContext(Forum forum, User user)
		{
			return GetPermissionContext(forum, user, null);
		}

		public ForumPermissionContext GetPermissionContext(Forum forum, User user, Topic topic)
		{
			var context = new ForumPermissionContext { DenialReason = String.Empty };
			var viewRestrictionRoles = _forumRepository.GetForumViewRoles(forum.ForumID);
			var postRestrictionRoles = _forumRepository.GetForumPostRoles(forum.ForumID);

			// view
			if (viewRestrictionRoles.Count == 0)
				context.UserCanView = true;
			else
			{
				context.UserCanView = false;
				if (user != null && viewRestrictionRoles.Where(user.IsInRole).Any())
					context.UserCanView = true;
			}

			// post
			if (user == null || !context.UserCanView)
			{
				context.UserCanPost = false;
				context.DenialReason = Resources.LoginToPost;
			}
			else
				if (!user.IsApproved)
				{
					context.DenialReason += "You can't post until you have verified your account. ";
					context.UserCanPost = false;
				}
				else
				{
					if (postRestrictionRoles.Count == 0)
						context.UserCanPost = true;
					else
					{
						if (postRestrictionRoles.Where(user.IsInRole).Count() > 0)
							context.UserCanPost = true;
						else
						{
							context.DenialReason += Resources.ForumNoPost + ". ";
							context.UserCanPost = false;
						}
					}
				}

			if (topic != null && topic.IsClosed)
			{
				context.UserCanPost = false;
				context.DenialReason = Resources.Closed + ". ";
			}

			if (topic != null && topic.IsDeleted)
			{
				if (user == null || !user.IsInRole(PermanentRoles.Moderator))
					context.UserCanView = false;
				context.DenialReason += "Topic is deleted. ";
			}

			if (forum.IsArchived)
			{
				context.UserCanPost = false;
				context.DenialReason += Resources.Archived + ". ";
			}
			
			// moderate
			context.UserCanModerate = false;
			if (user != null && (user.IsInRole(PermanentRoles.Admin) || user.IsInRole(PermanentRoles.Moderator)))
				context.UserCanModerate = true;

			return context;
		}

		public Topic PostNewTopic(Forum forum, User user, ForumPermissionContext permissionContext, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator)
		{
			if (!permissionContext.UserCanPost || !permissionContext.UserCanView)
				throw new Exception(String.Format("User {0} can't post to forum {1}.", user.Name, forum.Title));
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
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload {TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID});
			return topic;
		}

		private void ChangeForumSortOrder(Forum forum, int change)
		{
			var forums = _forumRepository.GetForumsInCategory(forum.CategoryID);
			forums.Single(c => c.ForumID == forum.ForumID).SortOrder += change;
			SortAndUpdateForums(forums);
		}

		private void SortAndUpdateForums(IEnumerable<Forum> forums)
		{
			var sorted = forums.OrderBy(f => f.SortOrder).ToList();
			for (var i = 0; i < sorted.Count; i++)
			{
				var correctedForum = sorted[i];
				correctedForum.SortOrder = i * 2;
				_forumRepository.UpdateSortOrder(correctedForum.ForumID, correctedForum.SortOrder);
			}
		}

		public void MoveForumUp(Forum forum)
		{
			const int change = -3;
			ChangeForumSortOrder(forum, change);
		}

		public void MoveForumUp(int forumID)
		{
			var forum = _forumRepository.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} doesn't exist, can't move it up.");
			MoveForumUp(forum);
		}

		public void MoveForumDown(int forumID)
		{
			var forum = _forumRepository.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} doesn't exist, can't move it down.");
			MoveForumDown(forum);
		}

		public void MoveForumDown(Forum forum)
		{
			const int change = 3;
			ChangeForumSortOrder(forum, change);
		}

		public List<string> GetForumPostRoles(Forum forum)
		{
			return _forumRepository.GetForumPostRoles(forum.ForumID);
		}

		public List<string> GetForumViewRoles(Forum forum)
		{
			return _forumRepository.GetForumViewRoles(forum.ForumID);
		}

		public void AddPostRole(Forum forum, string role)
		{
			_forumRepository.AddPostRole(forum.ForumID, role);
		}

		public void RemovePostRole(Forum forum, string role)
		{
			_forumRepository.RemovePostRole(forum.ForumID, role);
		}

		public void AddViewRole(Forum forum, string role)
		{
			_forumRepository.AddViewRole(forum.ForumID, role);
		}

		public void RemoveViewRole(Forum forum, string role)
		{
			_forumRepository.RemoveViewRole(forum.ForumID, role);
		}

		public void RemoveAllPostRoles(Forum forum)
		{
			_forumRepository.RemoveAllPostRoles(forum.ForumID);
		}

		public void RemoveAllViewRoles(Forum forum)
		{
			_forumRepository.RemoveAllViewRoles(forum.ForumID);
		}

		public Dictionary<int, string> GetAllForumTitles()
		{
			return _forumRepository.GetAllForumTitles();
		}

		public List<Topic> GetRecentTopics(User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var nonViewableForumIDs = GetNonViewableForumIDs(user);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = _topicRepository.Get(includeDeleted, nonViewableForumIDs, startRow, pageSize);
			var topicCount = _topicRepository.GetTopicCount(includeDeleted, nonViewableForumIDs);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public int GetAggregateTopicCount()
		{
			return _forumRepository.GetAggregateTopicCount();
		}

		public int GetAggregatePostCount()
		{
			return _forumRepository.GetAggregatePostCount();
		}

		public TopicContainerForQA MapTopicContainerForQA(TopicContainer topicContainer)
		{
			var result = new TopicContainerForQA
			{
				Forum = topicContainer.Forum,
				Topic = topicContainer.Topic,
				Posts = topicContainer.Posts,
				PagerContext = topicContainer.PagerContext,
				PermissionContext = topicContainer.PermissionContext,
				IsSubscribed = topicContainer.IsSubscribed,
				IsFavorite = topicContainer.IsFavorite,
				Signatures = topicContainer.Signatures,
				Avatars = topicContainer.Avatars,
				VotedPostIDs = topicContainer.VotedPostIDs
			};
			try
			{
				var questionPost = result.Posts.Single(x => x.IsFirstInTopic);
				var questionComments = result.Posts.Where(x => x.ParentPostID == questionPost.PostID).ToList();
				result.QuestionPostWithComments = new PostWithChildren { Post = questionPost, Children = questionComments, LastReadTime = topicContainer.LastReadTime };
			}
			catch (InvalidOperationException)
			{
				throw new InvalidOperationException(String.Format("There is no post marked as FirstInTopic for TopicID {0}.", topicContainer.Topic.TopicID));
			}
			var answers = result.Posts.Where(x => !x.IsFirstInTopic && (x.ParentPostID == 0)).OrderByDescending(x => x.Votes).ThenByDescending(x => x.PostTime).ToList();
			if (topicContainer.Topic.AnswerPostID.HasValue)
			{
				var acceptedAnswer = answers.SingleOrDefault(x => x.PostID == topicContainer.Topic.AnswerPostID.Value);
				if (acceptedAnswer != null)
				{
					answers.Remove(acceptedAnswer);
					answers.Insert(0, acceptedAnswer);
				}
			}
			result.AnswersWithComments = new List<PostWithChildren>();
			foreach (var item in answers)
			{
				var comments = result.Posts.Where(x => x.ParentPostID == item.PostID).ToList();
				result.AnswersWithComments.Add(new PostWithChildren { Post = item, Children = comments, LastReadTime = topicContainer.LastReadTime });
			}
			return result;
		}

		public void ModifyForumRoles(ModifyForumRolesContainer container)
		{
			var forum = Get(container.ForumID);
			if (forum == null)
				throw new Exception($"ForumID {container.ForumID} not found.");
			switch (container.ModifyType)
			{
				case ModifyForumRolesType.AddPost:
					AddPostRole(forum, container.Role);
					break;
				case ModifyForumRolesType.RemovePost:
					RemovePostRole(forum, container.Role);
					break;
				case ModifyForumRolesType.AddView:
					AddViewRole(forum, container.Role);
					break;
				case ModifyForumRolesType.RemoveView:
					RemoveViewRole(forum, container.Role);
					break;
				case ModifyForumRolesType.RemoveAllPost:
					RemoveAllPostRoles(forum);
					break;
				case ModifyForumRolesType.RemoveAllView:
					RemoveAllViewRoles(forum);
					break;
				default:
					throw new Exception("ModifyForumRoles doesn't know what to do.");
			}
		}
	}
}
