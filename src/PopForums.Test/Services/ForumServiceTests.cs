using System;
using System.Linq;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Test.Services
{
	public class ForumServiceTests
	{
		private Mock<IForumRepository> _mockForumRepo;
		private Mock<ITopicRepository> _mockTopicRepo;
		private Mock<ICategoryRepository> _mockCategoryRepo;
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<ILastReadService> _mockLastReadService;

		private ForumService GetService()
		{
			_mockCategoryRepo = new Mock<ICategoryRepository>();
			_mockForumRepo = new Mock<IForumRepository>();
			_mockTopicRepo = new Mock<ITopicRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockLastReadService = new Mock<ILastReadService>();
			return new ForumService(_mockForumRepo.Object, _mockTopicRepo.Object, _mockCategoryRepo.Object, _mockSettingsManager.Object, _mockLastReadService.Object);
		}

		[Fact]
		public async Task Get()
		{
			const int forumID = 123;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.Get(forumID)).ReturnsAsync(new Forum {ForumID = forumID});
			var forum = await forumService.Get(forumID);
			Assert.Equal(forumID, forum.ForumID);
			_mockForumRepo.Verify(f => f.Get(forumID), Times.Once());
		}

		[Fact]
		public async Task Create()
		{
			var forumService = GetService();
			const int categoryID = 456;
			const string title = "forum title";
			const string desc = "description of forum";
			const bool isVisible = true;
			const bool isArchived = true;
			const int sortOrder = 5;
			const int forumID = 123;
			const string adapter = "Jeff.Adapter";
			const bool isQAForum = true;
			var forum = new Forum {ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder};
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).ReturnsAsync(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetAll()).ReturnsAsync(new List<Forum> { new Forum { ForumID = 1, SortOrder = 9 }, new Forum { ForumID = 2, SortOrder = 6 }, forum});
			var result = await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			Assert.Equal(forum, result);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(123, 0), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(2, 2), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(1, 4), Times.Once());
		}

		[Fact]
		public async Task CreateMakesUrlTitle()
		{
			var forumService = GetService();
			const int categoryID = 456;
			const string title = "forum title";
			const string desc = "description of forum";
			const bool isVisible = true;
			const bool isArchived = true;
			const int sortOrder = 5;
			const int forumID = 123;
			const string adapter = "Jeff.Adapter";
			const bool isQAForum = true;
			var forum = new Forum { ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).ReturnsAsync(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title", adapter, isQAForum), Times.Once());
		}

		[Fact]
		public async Task CreateMakesUrlTitleWithAppendage()
		{
			var forumService = GetService();
			const int categoryID = 456;
			const string title = "forum title";
			const string desc = "description of forum";
			const bool isVisible = true;
			const bool isArchived = true;
			const int sortOrder = 5;
			const int forumID = 123;
			const string adapter = "Jeff.Adapter";
			const bool isQAForum = true;
			var forum = new Forum { ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).ReturnsAsync(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(title.ToUrlName())).ReturnsAsync(new List<string> {"forum-title", "forum-title-but-not", "forum-title-2"});
			await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title-3", adapter, isQAForum), Times.Once());
		}

		[Fact]
		public async Task UpdateLast()
		{
			const int forumID = 123;
			const int topicID = 456;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum { ForumID = forumID };
			var topic = new Topic { TopicID = topicID, LastPostTime = lastTime, LastPostName = lastName };
			var forumService = GetService();
			_mockTopicRepo.Setup(t => t.GetLastUpdatedTopic(forum.ForumID)).ReturnsAsync(topic);
			await forumService.UpdateLast(forum);
			_mockTopicRepo.Verify(t => t.GetLastUpdatedTopic(forum.ForumID), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName), Times.Once());
		}

		[Fact]
		public async Task UpdateLastWithValues()
		{
			var forumService = GetService();
			const int forumID = 123;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum { ForumID = forumID };
			await forumService.UpdateLast(forum, lastTime, lastName);
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName), Times.Once());
		}

		//[Fact]
		//[Ignore] // TODO: gotta account for spawned thread
		//public void UpdateCounts()
		//{
		//    const int topicCount = 456;
		//    const int postCount = 789;
		//    const int forumID = 123;
		//    var forum = new Forum(forumID);
		//    var forumService = GetService();
		//    _mockTopicRepo.Setup(t => t.GetPostCount(forumID, false)).Returns(postCount);
		//    _mockTopicRepo.Setup(t => t.GetTopicCount(forumID, false)).Returns(topicCount);
		//    forumService.UpdateCounts(forum);
		//    _mockTopicRepo.Verify(t => t.GetPostCount(forumID, false), Times.Once());
		//    _mockTopicRepo.Verify(t => t.GetTopicCount(forumID, false), Times.Once());
		//    _mockForumRepo.Verify(f => f.UpdateTopicAndPostCounts(forumID, topicCount, postCount));
		//}

		[Fact]
		public async Task GetForumsWithCategories()
		{
			var forums = new List<Forum>();
			var cats = new List<Category>();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetAll()).ReturnsAsync(forums);
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(cats);
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("whatever");
			var container = await forumService.GetCategorizedForumContainer();
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockForumRepo.Verify(f => f.GetAll(), Times.Once());
			Assert.Equal(container.AllForums, forums);
			Assert.Equal(container.AllCategories, cats);
		}

		[Fact]
		public async Task MoveUp()
		{
			var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum { ForumID = 1000,SortOrder = 6, CategoryID = 777 };
			var forums = new List<Forum> { f1, f2, f3, f4 };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumsInCategory(777)).ReturnsAsync(forums);
			_mockForumRepo.Setup(x => x.Get(f3.ForumID)).ReturnsAsync(f3);
			await service.MoveForumUp(f3.ForumID);
			_mockForumRepo.Verify(f => f.GetForumsInCategory(777), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(4));
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f1.ForumID, f1.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f2.ForumID, f2.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f3.ForumID, f3.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f4.ForumID, f4.SortOrder), Times.Once());
			Assert.Equal(0, f1.SortOrder);
			Assert.Equal(2, f3.SortOrder);
			Assert.Equal(4, f2.SortOrder);
			Assert.Equal(6, f4.SortOrder);
		}

		[Fact]
		public async Task MoveDown()
		{
			var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum { ForumID = 1000, SortOrder = 6, CategoryID = 777 };
			var forums = new List<Forum> { f1, f2, f3, f4 };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumsInCategory(777)).ReturnsAsync(forums);
			_mockForumRepo.Setup(x => x.Get(f3.ForumID)).ReturnsAsync(f3);
			await service.MoveForumDown(f3.ForumID);
			_mockForumRepo.Verify(f => f.GetForumsInCategory(777), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(4));
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f1.ForumID, f1.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f2.ForumID, f2.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f3.ForumID, f3.SortOrder), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(f4.ForumID, f4.SortOrder), Times.Once());
			Assert.Equal(0, f1.SortOrder);
			Assert.Equal(2, f2.SortOrder);
			Assert.Equal(4, f4.SortOrder);
			Assert.Equal(6, f3.SortOrder);
		}

		[Fact]
		public async Task MoveForumUpThrowsIfNoForum()
		{
			var service = GetService();
			_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum) null);

			await Assert.ThrowsAsync<Exception>(async () => await service.MoveForumUp(1));
		}

		[Fact]
		public async Task MoveForumDownThrowsIfNoForum()
		{
			var service = GetService();
			_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum)null);

			await Assert.ThrowsAsync<Exception>(async () => await service.MoveForumDown(1));
		}

		[Fact]
		public async Task PostRestrictions()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			var roles = new List<string> {"leader", "follower"};
			_mockForumRepo.Setup(f => f.GetForumPostRoles(forum.ForumID)).ReturnsAsync(roles);
			var result = await service.GetForumPostRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumPostRoles(forum.ForumID), Times.Once());
			Assert.Same(roles, result);
		}

		[Fact]
		public async Task ViewRestrictions()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			var roles = new List<string> { "leader", "follower" };
			_mockForumRepo.Setup(f => f.GetForumViewRoles(forum.ForumID)).ReturnsAsync(roles);
			var result = await service.GetForumViewRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumViewRoles(forum.ForumID), Times.Once());
			Assert.Same(roles, result);
		}

		[Fact]
		public async Task GetViewableForumIDsFromViewRestrictedForumsReturnsEmptyDictionaryWithoutUser()
		{
			var graph = new Dictionary<int, List<string>>
			{
				{1, new List<string> {"blah"}},
				{2, new List<string>()},
				{3, new List<string> {"blah"}}
			};
			var service = GetService();
			_mockForumRepo.Setup(x => x.GetAllVisible()).ReturnsAsync(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
			});
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			var result = await service.GetViewableForumIDsFromViewRestrictedForums(null);
			Assert.Single(result);
			Assert.Equal(2, result[0]);
		}

		[Fact]
		public async Task GetViewableForumIDsFromViewRestrictedForumsDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			_mockForumRepo.Setup(x => x.GetAllVisible()).ReturnsAsync(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
			});
			var result = await service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new [] {"blah"}.ToList() });
			Assert.Equal(3, result.Count);
		}

		[Fact]
		public async Task GetViewableForumIDsFromViewRestrictedForumsReturnsIDsWithMatchingUserRoles()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blep" });
			graph.Add(4, new List<string> { "burp", "blah" });
			graph.Add(5, new List<string> { "burp" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			_mockForumRepo.Setup(x => x.GetAllVisible()).ReturnsAsync(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }, new Forum { ForumID = 4 }, new Forum { ForumID = 5 }
			});
			var result = await service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new[] { "blah", "blep" }.ToList() });
			Assert.Equal(4, result.Count);
			Assert.Contains(1, result);
			Assert.Contains(2, result);
			Assert.Contains(3, result);
			Assert.Contains(4, result);
			Assert.DoesNotContain(5, result);
		}

		[Fact]
		public async Task GetNonViewableDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string>()});
			Assert.Equal(2, result.Count);
			Assert.DoesNotContain(2, result);
		}

		[Fact]
		public async Task GetNonViewableDoesntIncludeForumsWithRoleMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Single(result);
			Assert.DoesNotContain(3, result);
		}

		[Fact]
		public async Task GetNonViewableIncludesForumsWithNoMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Single(result);
			Assert.Equal(1, result[0]);
		}

		[Fact]
		public async Task GetNonViewableExcludesViewRestrictionsForNoUser()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			var result = await service.GetNonViewableForumIDs(null);
			Assert.Equal(2, result.Count);
			Assert.Equal(1, result[0]);
			Assert.Equal(3, result[1]);
		}

		[Fact]
		public async Task GetCategorizedForUserHasOnlyViewableForums()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var allForums = new List<Forum> {new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 } };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(graph);
			_mockForumRepo.Setup(f => f.GetAllVisible()).ReturnsAsync(allForums);
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(new List<Category>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("whatever");
			var container = await service.GetCategorizedForumContainerFilteredForUser(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Equal(2, container.UncategorizedForums.Count);
			Assert.Null(container.UncategorizedForums.SingleOrDefault(f => f.ForumID == 1));
		}

		[Fact]
		public async Task GetCategorizedForUserPopulatesReadStatus()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(new List<Category>());
			_mockForumRepo.Setup(f => f.GetAllVisible()).ReturnsAsync(new List<Forum>());
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).ReturnsAsync(new Dictionary<int, List<string>>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("");
			await service.GetCategorizedForumContainerFilteredForUser(user);
			_mockLastReadService.Verify(l => l.GetForumReadStatus(user, It.IsAny<CategorizedForumContainer>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetCategoryContainersWithForumsMapsCatsWithUnCatForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).ReturnsAsync(categories);
			var forums = new List<Forum>
			{
				new Forum {ForumID = 1, CategoryID = null},
				new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
				new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
				new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
				new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
				new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
			};
			_mockForumRepo.Setup(x => x.GetAll()).ReturnsAsync(forums);

			var result = await service.GetCategoryContainersWithForums();

			Assert.Equal(0, result[0].Category.CategoryID);
			Assert.Equal(2, result[1].Category.CategoryID);
			Assert.Equal(3, result[2].Category.CategoryID);
			Assert.Equal(1, result[3].Category.CategoryID);
		}

		[Fact]
		public async Task GetCategoryContainersWithForumsMapsCatsWithoutUnCatForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).ReturnsAsync(categories);
			var forums = new List<Forum>
			{
				new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
				new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
				new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
				new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
				new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
			};
			_mockForumRepo.Setup(x => x.GetAll()).ReturnsAsync(forums);

			var result = await service.GetCategoryContainersWithForums();

			Assert.Equal(2, result[0].Category.CategoryID);
			Assert.Equal(3, result[1].Category.CategoryID);
			Assert.Equal(1, result[2].Category.CategoryID);
		}

		[Fact]
		public async Task GetCategoryContainersWithForumsMapsForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).ReturnsAsync(categories);
			var forums = new List<Forum>
			{
				new Forum {ForumID = 1, CategoryID = null, SortOrder = 3},
				new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
				new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
				new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
				new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
				new Forum {ForumID = 6, CategoryID = categories[2].CategoryID},
				new Forum {ForumID = 7, CategoryID = null, SortOrder = 1},
			};
			_mockForumRepo.Setup(x => x.GetAll()).ReturnsAsync(forums);

			var result = await service.GetCategoryContainersWithForums();

			Assert.Equal(7, result[0].Forums.ToArray()[0].ForumID);
			Assert.Equal(1, result[0].Forums.ToArray()[1].ForumID);
			Assert.Equal(3, result[3].Forums.ToArray()[0].ForumID);
			Assert.Equal(2, result[3].Forums.ToArray()[1].ForumID);
			Assert.Equal(5, result[3].Forums.ToArray()[2].ForumID);
			Assert.Equal(4, result[3].Forums.ToArray()[3].ForumID);
			Assert.Equal(6, result[2].Forums.ToArray()[0].ForumID);
		}

		[Fact]
		public void MapTopicContainerForQAMapsBaseProperties()
		{
			var topicContainer = new TopicContainer
			{
				Forum = new Forum { ForumID = 1 },
				Topic = new Topic { TopicID = 2 },
				Posts = new List<Post> {new Post { PostID = 123, IsFirstInTopic = true }},
				PagerContext = new PagerContext(),
				PermissionContext = new ForumPermissionContext(),
				IsSubscribed = true,
				IsFavorite = true,
				Signatures = new Dictionary<int, string>(),
				Avatars = new Dictionary<int, int>(),
				VotedPostIDs = new List<int>()
			};
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Same(topicContainer.Forum, result.Forum);
			Assert.Same(topicContainer.Topic, result.Topic);
			Assert.Same(topicContainer.Posts, result.Posts);
			Assert.Same(topicContainer.PagerContext, result.PagerContext);
			Assert.Same(topicContainer.PermissionContext, result.PermissionContext);
			Assert.True(topicContainer.IsSubscribed);
			Assert.True(topicContainer.IsFavorite);
			Assert.Same(topicContainer.Signatures, result.Signatures);
			Assert.Same(topicContainer.Avatars, result.Avatars);
			Assert.Same(topicContainer.VotedPostIDs, result.VotedPostIDs);
		}

		[Fact]
		public void MapTopicContainerGrabsFirstPostForQuestion()
		{
			var posts = new List<Post>
			{
				new Post {PostID = 1},
				new Post{PostID = 2, IsFirstInTopic = true}
			};
			var topicContainer = new TopicContainer {Posts = posts, Topic = new Topic { TopicID = 123 }};
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Equal(2, result.QuestionPostWithComments.Post.PostID);
		}

		[Fact]
		public void MapTopicContainerThrowsWithNoFirstInTopicPost()
		{
			var posts = new List<Post>
			{
				new Post { PostID =  1 },
				new Post { PostID =  2 }
			};
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 123 } };
			var service = GetService();
			Assert.Throws<InvalidOperationException>(() => service.MapTopicContainerForQA(topicContainer));
		}

		[Fact]
		public void MapTopicContainerThrowsWithMoreThanOneFirstInTopicPost()
		{
			var posts = new List<Post>
			{
				new Post { PostID =  1, IsFirstInTopic = true},
				new Post { PostID =  2, IsFirstInTopic = true}
			};
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 123 } };
			var service = GetService();
			Assert.Throws<InvalidOperationException>(() => service.MapTopicContainerForQA(topicContainer));
		}

		[Fact]
		public void MapTopicContainerSetsQuestionsWithNoParentAsAnswers()
		{
			var post1 = new Post { PostID = 1, ParentPostID = 0};
			var post2 = new Post { PostID = 2, IsFirstInTopic = true};
			var post3 = new Post { PostID = 3, ParentPostID = 2};
			var post4 = new Post { PostID = 4, ParentPostID = 1};
			var post5 = new Post { PostID = 5, ParentPostID = 3};
			var posts = new List<Post> {post1, post2, post3, post4, post5};
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Single(result.AnswersWithComments);
			Assert.Same(post1, result.AnswersWithComments[0].Post);
		}

		[Fact]
		public void MapTopicContainerMapsCommentsToParentQuestionsAndAnswers()
		{
			var post1 = new Post { PostID = 1, ParentPostID = 0 };
			var post2 = new Post { PostID = 2, IsFirstInTopic = true };
			var post3 = new Post { PostID = 3, ParentPostID = 0 };
			var post4 = new Post { PostID = 4, ParentPostID = 1 };
			var post5 = new Post { PostID = 5, ParentPostID = 2 };
			var post6 = new Post { PostID = 6, ParentPostID = 3 };
			var post7 = new Post { PostID = 7, ParentPostID = 3 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.True(result.AnswersWithComments[0].Children.Count == 1);
			Assert.Contains(post4, result.AnswersWithComments[0].Children);
			Assert.True(result.AnswersWithComments[1].Children.Count == 2);
			Assert.Contains(post6, result.AnswersWithComments[1].Children);
			Assert.Contains(post7, result.AnswersWithComments[1].Children);
		}

		[Fact]
		public void MapTopicContainerMapsCommentsToQuestion()
		{
			var post1 = new Post { PostID = 1, ParentPostID = 0 };
			var post2 = new Post { PostID = 2, IsFirstInTopic = true };
			var post3 = new Post { PostID = 3, ParentPostID = 0 };
			var post4 = new Post { PostID = 4, ParentPostID = 1 };
			var post5 = new Post { PostID = 5, ParentPostID = 2 };
			var post6 = new Post { PostID = 6, ParentPostID = 2 };
			var post7 = new Post { PostID = 7, ParentPostID = 3 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.True(result.QuestionPostWithComments.Children.Count == 2);
			Assert.Contains(post5, result.QuestionPostWithComments.Children);
			Assert.Contains(post6, result.QuestionPostWithComments.Children);
		}

		[Fact]
		public void MapTopicContainerOrdersAnswersByVoteThenDate()
		{
			var post1 = new Post { PostID = 1, IsFirstInTopic = true };
			var post2 = new Post { PostID = 2, Votes = 7, PostTime = new DateTime(2000, 1, 1) };
			var post3 = new Post { PostID = 3, Votes = 7, PostTime = new DateTime(2000, 2, 1) };
			var post4 = new Post { PostID = 4, Votes = 2 };
			var post5 = new Post { PostID = 5, Votes = 3 };
			var post6 = new Post { PostID = 6, Votes = 8 };
			var post7 = new Post { PostID = 7, Votes = 5 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var topic = new Topic { TopicID = 123, AnswerPostID = null };
			var topicContainer = new TopicContainer { Posts = posts, Topic = topic };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Same(post6, result.AnswersWithComments[0].Post);
			Assert.Same(post3, result.AnswersWithComments[1].Post);
			Assert.Same(post2, result.AnswersWithComments[2].Post);
			Assert.Same(post7, result.AnswersWithComments[3].Post);
			Assert.Same(post5, result.AnswersWithComments[4].Post);
			Assert.Same(post4, result.AnswersWithComments[5].Post);
		}

		[Fact]
		public void MapTopicContainerOrdersAnswersByAnswerThenVoteThenDate()
		{
			var post1 = new Post { PostID = 1, IsFirstInTopic = true };
			var post2 = new Post { PostID = 2, Votes = 7, PostTime = new DateTime(2000, 1, 1) };
			var post3 = new Post { PostID = 3, Votes = 7, PostTime = new DateTime(2000, 2, 1) };
			var post4 = new Post { PostID = 4, Votes = 2 };
			var post5 = new Post { PostID = 5, Votes = 3 };
			var post6 = new Post { PostID = 6, Votes = 8 };
			var post7 = new Post { PostID = 7, Votes = 5 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var topic = new Topic { TopicID = 123, AnswerPostID = 5};
			var topicContainer = new TopicContainer { Posts = posts, Topic = topic };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Same(post5, result.AnswersWithComments[0].Post);
			Assert.Same(post6, result.AnswersWithComments[1].Post);
			Assert.Same(post3, result.AnswersWithComments[2].Post);
			Assert.Same(post2, result.AnswersWithComments[3].Post);
			Assert.Same(post7, result.AnswersWithComments[4].Post);
			Assert.Same(post4, result.AnswersWithComments[5].Post);
		}

		[Fact]
		public void MapTopicContainerDoesNotMapCommentsForTopQuestionAsReplies()
		{
			var post1 = new Post { PostID = 1, ParentPostID = 0 };
			var post2 = new Post { PostID = 2, IsFirstInTopic = true };
			var post3 = new Post { PostID = 3, ParentPostID = 0 };
			var post4 = new Post { PostID = 4, ParentPostID = 1 };
			var post5 = new Post { PostID = 5, ParentPostID = 2 };
			var post6 = new Post { PostID = 6, ParentPostID = 3 };
			var post7 = new Post { PostID = 7, ParentPostID = 3 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.DoesNotContain(result.AnswersWithComments, x => x.Post.PostID == post5.PostID);
		}

		[Fact]
		public void MapTopicContainerMapsLastReadTimeToQuestionAndAnswerSets()
		{
			var post1 = new Post { PostID = 1, ParentPostID = 0 };
			var post2 = new Post { PostID = 2, IsFirstInTopic = true };
			var post3 = new Post { PostID = 3, ParentPostID = 0 };
			var post4 = new Post { PostID = 4, ParentPostID = 1 };
			var post5 = new Post { PostID = 5, ParentPostID = 2 };
			var post6 = new Post { PostID = 6, ParentPostID = 3 };
			var post7 = new Post { PostID = 7, ParentPostID = 3 };
			var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
			var lastRead = new DateTime(2000, 1, 1);
			var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 }, LastReadTime = lastRead };
			var service = GetService();
			var result = service.MapTopicContainerForQA(topicContainer);
			Assert.Equal(lastRead, result.AnswersWithComments[0].LastReadTime);
			Assert.Equal(lastRead, result.AnswersWithComments[1].LastReadTime);
			Assert.Equal(lastRead, result.QuestionPostWithComments.LastReadTime);
		}

		public class ModifyForumRoles : ForumServiceTests
		{
			[Fact]
			public async Task ThrowsIfNoForumMatch()
			{
				var service = GetService();
				_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum) null);

				await Assert.ThrowsAsync<Exception>(async () => await service.ModifyForumRoles(new ModifyForumRolesContainer()));
			}

			private async Task<Tuple<int, string>> CallSetup(ModifyForumRolesType modifyType)
			{
				var service = GetService();
				var forum = new Forum { ForumID = 123 };
				var role = "role";
				_mockForumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				await service.ModifyForumRoles(new ModifyForumRolesContainer { ForumID = forum.ForumID, ModifyType = modifyType, Role = role });
				return Tuple.Create(forum.ForumID, role);
			}

			[Fact]
			public async Task AddPostCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.AddPost);

				_mockForumRepo.Verify(x => x.AddPostRole(forumID, role), Times.Once);
			}

			[Fact]
			public async Task RemovePostCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.RemovePost);

				_mockForumRepo.Verify(x => x.RemovePostRole(forumID, role), Times.Once);
			}

			[Fact]
			public async Task AddViewCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.AddView);

				_mockForumRepo.Verify(x => x.AddViewRole(forumID, role), Times.Once);
			}

			[Fact]
			public async Task RemoveViewCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.RemoveView);

				_mockForumRepo.Verify(x => x.RemoveViewRole(forumID, role), Times.Once);
			}

			[Fact]
			public async Task RemoveAllPostCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.RemoveAllPost);

				_mockForumRepo.Verify(x => x.RemoveAllPostRoles(forumID), Times.Once);
			}

			[Fact]
			public async Task RemoveAllViewCallsRepo()
			{
				var (forumID, role) = await CallSetup(ModifyForumRolesType.RemoveAllView);

				_mockForumRepo.Verify(x => x.RemoveAllViewRoles(forumID), Times.Once);
			}
		}
	}
}
