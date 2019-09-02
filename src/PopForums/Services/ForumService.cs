using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IForumService
	{
		Task<Forum> Get(int forumID);
		Task<Forum> Get(string urlName);
		Task<Forum> Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName, bool isQAForum);
		Task UpdateLast(Forum forum);
		Task UpdateLast(Forum forum, DateTime lastTime, string lastName);
		void UpdateCounts(Forum forum);
		Task<CategorizedForumContainer> GetCategorizedForumContainer();
		Task<List<CategoryContainerWithForums>> GetCategoryContainersWithForums();
		Task<CategorizedForumContainer> GetCategorizedForumContainerFilteredForUser(User user);
		Task<List<int>> GetNonViewableForumIDs(User user);
		Task Update(Forum forum, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum);
		Task MoveForumUp(int forumID);
		Task MoveForumDown(int forumID);
		Task<List<string>> GetForumPostRoles(Forum forum);
		Task<List<string>> GetForumViewRoles(Forum forum);
		Dictionary<int, string> GetAllForumTitles();
		Task<Tuple<List<Topic>, PagerContext>> GetRecentTopics(User user, bool includeDeleted, int pageIndex);
		Task<int> GetAggregateTopicCount();
		Task<int> GetAggregatePostCount();
		Task<List<int>> GetViewableForumIDsFromViewRestrictedForums(User user);
		TopicContainerForQA MapTopicContainerForQA(TopicContainer topicContainer);
		Task ModifyForumRoles(ModifyForumRolesContainer container);
	}

	public class ForumService : IForumService
	{
		public ForumService(IForumRepository forumRepository, ITopicRepository topicRepository, ICategoryRepository categoryRepository, ISettingsManager settingsManager, ILastReadService lastReadService)
		{
			_forumRepository = forumRepository;
			_topicRepository = topicRepository;
			_categoryRepository = categoryRepository;
			_settingsManager = settingsManager;
			_lastReadService = lastReadService;
		}

		private readonly IForumRepository _forumRepository;
		private readonly ITopicRepository _topicRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly ILastReadService _lastReadService;

		public async Task<Forum> Get(int forumID)
		{
			return await _forumRepository.Get(forumID);
		}

		public async Task<Forum> Get(string urlName)
		{
			return await _forumRepository.Get(urlName);
		}

		public async Task<Forum> Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName, bool isQAForum)
		{
			var urlName = title.ToUniqueUrlName(await _forumRepository.GetUrlNamesThatStartWith(title.ToUrlName()));
			var forum = await _forumRepository.Create(categoryID, title, description, isVisible, isArchived, sortOrder, urlName, forumAdapterName, isQAForum);
			forum.UrlName = urlName;
			var forums = await _forumRepository.GetAll();
			await SortAndUpdateForums(forums.ToList());
			return forum;
		}

		public async Task Update(Forum forum, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
		{
			var urlName = forum.UrlName;
			if (forum.Title != title)
				urlName = title.ToUniqueUrlName(await _forumRepository.GetUrlNamesThatStartWith(title.ToUrlName()));
			await _forumRepository.Update(forum.ForumID, categoryID, title, description, isVisible, isArchived, urlName, forumAdapterName, isQAForum);
		}

		public async Task UpdateLast(Forum forum)
		{
			var topic = await _topicRepository.GetLastUpdatedTopic(forum.ForumID);
			if (topic != null)
				await UpdateLast(forum, topic.LastPostTime, topic.LastPostName);
			else
				await UpdateLast(forum, new DateTime(2000, 1, 1), String.Empty);
		}

		public async Task UpdateLast(Forum forum, DateTime lastTime, string lastName)
		{
			await _forumRepository.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName);
		}

		public void UpdateCounts(Forum forum)
		{
			new Thread(() =>
			{
				var topicCount = _topicRepository.GetTopicCount(forum.ForumID, false).Result;
				var postCount = _topicRepository.GetPostCount(forum.ForumID, false).Result;
				_forumRepository.UpdateTopicAndPostCounts(forum.ForumID, topicCount, postCount);
			}).Start();
		}

		public async Task<CategorizedForumContainer> GetCategorizedForumContainer()
		{
			var forums = _forumRepository.GetAll();
			var categories = await _categoryRepository.GetAll();
			var container = new CategorizedForumContainer(categories, forums.Result);
			container.ForumTitle = _settingsManager.Current.ForumTitle;
			return container;
		}

		public async Task<List<CategoryContainerWithForums>> GetCategoryContainersWithForums()
		{
			var containers = new List<CategoryContainerWithForums>();
			var forumResult = await _forumRepository.GetAll();
			var forums = forumResult.ToList();
			var categories = await _categoryRepository.GetAll();
			var orderedCategories = categories.OrderBy(x => x.SortOrder);
			var uncategorized = forums.Where(x => !x.CategoryID.HasValue).OrderBy(x => x.SortOrder).ToList();
			if (uncategorized.Count > 0)
				containers.Add(new CategoryContainerWithForums {Category = new Category { Title = Resources.ForumsUncat }, Forums = uncategorized});
			foreach (var item in orderedCategories)
			{
				var filteredForums = forums.Where(x => x.CategoryID == item.CategoryID).OrderBy(x => x.SortOrder);
				containers.Add(new CategoryContainerWithForums { Category = item, Forums = filteredForums });
			}
			return containers;
		}

		public async Task<List<int>> GetViewableForumIDsFromViewRestrictedForums(User user)
		{
			var nonViewableForumIDs = await GetNonViewableForumIDs(user);
			var forums = await _forumRepository.GetAllVisible();
			var noViewRestrictionForums = forums.Where(f => !nonViewableForumIDs.Contains(f.ForumID));
			return noViewRestrictionForums.Select(x => x.ForumID).ToList();
		}

		public async Task<CategorizedForumContainer> GetCategorizedForumContainerFilteredForUser(User user)
		{
			var nonViewableForumIDs = await GetNonViewableForumIDs(user);
			var unfilteredForums = await _forumRepository.GetAllVisible();
			var forums = unfilteredForums.Where(f => !nonViewableForumIDs.Contains(f.ForumID));
			var categories = await _categoryRepository.GetAll();
			var container = new CategorizedForumContainer(categories, forums);
			await _lastReadService.GetForumReadStatus(user, container);
			container.ForumTitle = _settingsManager.Current.ForumTitle;
			return container;
		}

		public async Task<List<int>> GetNonViewableForumIDs(User user)
		{
			var forumsWithRestrictions = await _forumRepository.GetForumViewRestrictionRoleGraph();
			var nonViewableForums = new List<int>();
			foreach (var item in forumsWithRestrictions.Where(f => f.Value.Count > 0))
			{
				if (user == null)
					nonViewableForums.Add(item.Key);
				else if (!user.Roles.Intersect(item.Value).Any())
					nonViewableForums.Add(item.Key);
			}
			return nonViewableForums;
		}

		private async Task ChangeForumSortOrder(Forum forum, int change)
		{
			var forums = await _forumRepository.GetForumsInCategory(forum.CategoryID);
			forums.Single(c => c.ForumID == forum.ForumID).SortOrder += change;
			await SortAndUpdateForums(forums);
		}

		private async Task SortAndUpdateForums(IEnumerable<Forum> forums)
		{
			var sorted = forums.OrderBy(f => f.SortOrder).ToList();
			for (var i = 0; i < sorted.Count; i++)
			{
				var correctedForum = sorted[i];
				correctedForum.SortOrder = i * 2;
				await _forumRepository.UpdateSortOrder(correctedForum.ForumID, correctedForum.SortOrder);
			}
		}

		public async Task MoveForumUp(int forumID)
		{
			var forum = await _forumRepository.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} doesn't exist, can't move it up.");
			const int change = -3;
			await ChangeForumSortOrder(forum, change);
		}

		public async Task MoveForumDown(int forumID)
		{
			var forum = await _forumRepository.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} doesn't exist, can't move it down.");
			const int change = 3;
			await ChangeForumSortOrder(forum, change);
		}

		public async Task<List<string>> GetForumPostRoles(Forum forum)
		{
			return await _forumRepository.GetForumPostRoles(forum.ForumID);
		}

		public async Task<List<string>> GetForumViewRoles(Forum forum)
		{
			return await _forumRepository.GetForumViewRoles(forum.ForumID);
		}

		public Dictionary<int, string> GetAllForumTitles()
		{
			return _forumRepository.GetAllForumTitles();
		}

		public async Task<Tuple<List<Topic>, PagerContext>> GetRecentTopics(User user, bool includeDeleted, int pageIndex)
		{
			var nonViewableForumIDs = await GetNonViewableForumIDs(user);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = await _topicRepository.Get(includeDeleted, nonViewableForumIDs, startRow, pageSize);
			var topicCount = await _topicRepository.GetTopicCount(includeDeleted, nonViewableForumIDs);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return Tuple.Create(topics, pagerContext);
		}

		public async Task<int> GetAggregateTopicCount()
		{
			return await _forumRepository.GetAggregateTopicCount();
		}

		public async Task<int> GetAggregatePostCount()
		{
			return await _forumRepository.GetAggregatePostCount();
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
				throw new InvalidOperationException($"There is no post marked as FirstInTopic for TopicID {topicContainer.Topic.TopicID}.");
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

		public async Task ModifyForumRoles(ModifyForumRolesContainer container)
		{
			var forum = await Get(container.ForumID);
			if (forum == null)
				throw new Exception($"ForumID {container.ForumID} not found.");
			switch (container.ModifyType)
			{
				case ModifyForumRolesType.AddPost:
					await _forumRepository.AddPostRole(forum.ForumID, container.Role);
					break;
				case ModifyForumRolesType.RemovePost:
					await _forumRepository.RemovePostRole(forum.ForumID, container.Role);
					break;
				case ModifyForumRolesType.AddView:
					await _forumRepository.AddViewRole(forum.ForumID, container.Role);
					break;
				case ModifyForumRolesType.RemoveView:
					await _forumRepository.RemoveViewRole(forum.ForumID, container.Role);
					break;
				case ModifyForumRolesType.RemoveAllPost:
					await _forumRepository.RemoveAllPostRoles(forum.ForumID);
					break;
				case ModifyForumRolesType.RemoveAllView:
					await _forumRepository.RemoveAllViewRoles(forum.ForumID);
					break;
				default:
					throw new Exception("ModifyForumRoles doesn't know what to do.");
			}
		}
	}
}
