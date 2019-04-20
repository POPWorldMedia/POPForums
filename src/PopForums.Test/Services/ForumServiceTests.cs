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

namespace PopForums.Test.Services
{
	public class ForumServiceTests
	{
		private Mock<IForumRepository> _mockForumRepo;
		private Mock<ITopicRepository> _mockTopicRepo;
		private Mock<IPostRepository> _mockPostRepo;
		private Mock<ICategoryRepository> _mockCategoryRepo;
		private Mock<IProfileRepository> _mockProfileRepo;
		private Mock<ITextParsingService> _mockTextParser;
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<ILastReadService> _mockLastReadService;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<IBroker> _broker;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;
		private Mock<ITenantService> _tenantService;

		private ForumService GetService()
		{
			_mockCategoryRepo = new Mock<ICategoryRepository>();
			_mockForumRepo = new Mock<IForumRepository>();
			_mockTopicRepo = new Mock<ITopicRepository>();
			_mockPostRepo = new Mock<IPostRepository>();
			_mockProfileRepo = new Mock<IProfileRepository>();
			_mockTextParser = new Mock<ITextParsingService>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockLastReadService = new Mock<ILastReadService>();
			_eventPublisher = new Mock<IEventPublisher>();
			_broker = new Mock<IBroker>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			return new ForumService(_mockForumRepo.Object, _mockTopicRepo.Object, _mockPostRepo.Object, _mockCategoryRepo.Object, _mockProfileRepo.Object, _mockTextParser.Object, _mockSettingsManager.Object, _mockLastReadService.Object, _eventPublisher.Object, _broker.Object, _searchIndexQueueRepo.Object, _tenantService.Object);
		}

		private User DoUpNewTopic()
		{
			var forum = new Forum {ForumID = 1};
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var forumService = GetService();
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_mockPostRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<int>())).Returns(69);
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(x => x.Create(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(111);
			forumService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			return user;
		}

		[Fact]
		public void Get()
		{
			const int forumID = 123;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.Get(forumID)).Returns(new Forum {ForumID = forumID});
			var forum = forumService.Get(forumID);
			Assert.Equal(forumID, forum.ForumID);
			_mockForumRepo.Verify(f => f.Get(forumID), Times.Once());
		}

		[Fact]
		public void Create()
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
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetAll()).Returns(new List<Forum> { new Forum { ForumID = 1, SortOrder = 9 }, new Forum { ForumID = 2, SortOrder = 6 }, forum});
			var result = forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			Assert.Equal(forum, result);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(123, 0), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(2, 2), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(1, 4), Times.Once());
		}

		[Fact]
		public void CreateMakesUrlTitle()
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
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title", adapter, isQAForum), Times.Once());
		}

		[Fact]
		public void CreateMakesUrlTitleWithAppendage()
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
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter, isQAForum)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(title.ToUrlName())).Returns(new List<string> {"forum-title", "forum-title-but-not", "forum-title-2"});
			forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title-3", adapter, isQAForum), Times.Once());
		}

		[Fact]
		public void UpdateLast()
		{
			const int forumID = 123;
			const int topicID = 456;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum { ForumID = forumID };
			var topic = new Topic { TopicID = topicID, LastPostTime = lastTime, LastPostName = lastName };
			var forumService = GetService();
			_mockTopicRepo.Setup(t => t.GetLastUpdatedTopic(forum.ForumID)).Returns(topic);
			forumService.UpdateLast(forum);
			_mockTopicRepo.Verify(t => t.GetLastUpdatedTopic(forum.ForumID), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName), Times.Once());
		}

		[Fact]
		public void UpdateLastWithValues()
		{
			var forumService = GetService();
			const int forumID = 123;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum { ForumID = forumID };
			forumService.UpdateLast(forum, lastTime, lastName);
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
		public void IncrementPostCount()
		{
			const int forumID = 123;
			var forum = new Forum { ForumID = forumID };
			var forumService = GetService();
			forumService.IncrementPostCount(forum);
			_mockForumRepo.Verify(f => f.IncrementPostCount(forumID), Times.Once());
		}

		[Fact]
		public void IncrementTopicAndPostCount()
		{
			const int forumID = 123;
			var forum = new Forum { ForumID = forumID };
			var forumService = GetService();
			forumService.IncrementPostAndTopicCount(forum);
			_mockForumRepo.Verify(f => f.IncrementPostAndTopicCount(forumID), Times.Once());
		}

		[Fact]
		public void GetForumsWithCategories()
		{
			var forums = new List<Forum>();
			var cats = new List<Category>();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetAll()).Returns(forums);
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(cats);
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("whatever");
			var container = forumService.GetCategorizedForumContainer();
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockForumRepo.Verify(f => f.GetAll(), Times.Once());
			Assert.Equal(container.AllForums, forums);
			Assert.Equal(container.AllCategories, cats);
		}

		private User GetUser()
		{
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string>();
			return user;
		}

		[Fact]
		public void GetPermissionNoViewRestrictionWithUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.True(permission.UserCanView);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public void GetPermissionNoViewRestrictionWithoutUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public void GetPermissionViewRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>{"blah"});
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public void GetPermissionViewRestrictionUserCantPostEither()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public void GetPermissionViewRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public void GetPermissionViewRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public void GetPermissionPostRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public void GetPermissionPostRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanPost);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public void GetPermissionPostRestrictionUserNotApproved()
		{
			var user = GetUser();
			user.IsApproved = false;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public void GetPermissionPostRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public void GetPermissionModerateNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanModerate);
		}

		[Fact]
		public void GetPermissionModerateUserIsAdmin()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Admin);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public void GetPermissionModerateUserIsModerator()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public void GetPermissionTopicClosed()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = true});
			Assert.False(premission.UserCanPost);
		}

		[Fact]
		public void GetPermissionTopicOpen()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = false });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public void GetPermissionWithUserTopicDeleted()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public void GetPermissionAnonTopicDeleted()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public void GetPermissionModOnTopicDeleted()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.True(premission.UserCanView);
		}

		[Fact]
		public void GetPermissionForumNotArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = false }, user, new Topic { TopicID = 4 });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public void GetPermissionForumIsArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = true }, user, new Topic { TopicID = 4 });
			Assert.False(premission.UserCanPost);
		}

		[Fact]
		public void UserWithoutPermissionThrowsOnPost()
		{
			var topicService = GetService();
			Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanPost = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
			Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanView = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
		}

		[Fact]
		public void PostNewTopicCallsTopicRepoCreate()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_mockTopicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
		}

		[Fact]
		public void PostNewTopicCallsForumTopicPostIncrement()
		{
			DoUpNewTopic();
			_mockForumRepo.Verify(f => f.IncrementPostAndTopicCount(1), Times.Once());
		}

		[Fact]
		public void PostNewTopicCallsForumUpdateLastUser()
		{
			var user = DoUpNewTopic();
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(1, It.IsAny<DateTime>(), user.Name), Times.Once());
		}

		[Fact]
		public void PostNewTopicCallsProfileSetLastPost()
		{
			var user = DoUpNewTopic();
			_mockProfileRepo.Verify(p => p.SetLastPostID(user.UserID, 69), Times.Once());
		}

		[Fact]
		public void PostNewTopicPublishesNewTopicEvent()
		{
			var user = DoUpNewTopic();
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false), Times.Once());
		}

		[Fact]
		public void PostNewTopicPublishesNewPostEvent()
		{
			var user = DoUpNewTopic();
			_eventPublisher.Verify(x => x.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
		}

		[Fact]
		public void PostNewTopicCallsBroker()
		{
			DoUpNewTopic();
			_broker.Verify(x => x.NotifyForumUpdate(It.IsAny<Forum>()), Times.Once());
			_broker.Verify(x => x.NotifyTopicUpdate(It.IsAny<Topic>(), It.IsAny<Forum>(), It.IsAny<string>()), Times.Once());
		}

		[Fact]
		public void PostNewTopicQueuesTopicForIndexing()
		{
			DoUpNewTopic();
			_tenantService.Setup(x => x.GetTenant()).Returns("");
			_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
		}

		[Fact]
		public void PostNewTopicDoesNotPublishToFeedIfForumHasViewRestrictions()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string> { "Admin" });
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_mockTopicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
			var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true), Times.Once());
		}

		[Fact]
		public void PostNewTopicReturnsTopic()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_mockTopicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
			var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			Assert.Equal(2, topic.TopicID);
			Assert.Equal(forum.ForumID, topic.ForumID);
			Assert.Equal("parsed title", topic.Title);
			Assert.Equal(0, topic.ReplyCount);
			Assert.Equal(0, topic.ViewCount);
			Assert.Equal(user.UserID, topic.StartedByUserID);
			Assert.Equal(user.Name, topic.StartedByName);
			Assert.Equal(user.UserID, topic.LastPostUserID);
			Assert.Equal(user.Name, topic.LastPostName);
			Assert.False(topic.IsClosed);
			Assert.False(topic.IsDeleted);
			Assert.False(topic.IsPinned);
			Assert.Equal("parsed-title", topic.UrlName);
		}

		[Fact]
		public void MoveUp()
		{
			var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum { ForumID = 1000,SortOrder = 6, CategoryID = 777 };
			var forums = new List<Forum> { f1, f2, f3, f4 };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumsInCategory(777)).Returns(forums);
			service.MoveForumUp(f3);
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
		public void MoveDown()
		{
			var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum { ForumID = 1000, SortOrder = 6, CategoryID = 777 };
			var forums = new List<Forum> { f1, f2, f3, f4 };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumsInCategory(777)).Returns(forums);
			service.MoveForumDown(f3);
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
		public void MoveForumUpThrowsIfNoForum()
		{
			var service = GetService();
			_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).Returns((Forum) null);

			Assert.Throws<Exception>(() => service.MoveForumUp(1));
		}

		[Fact]
		public void MoveForumDownThrowsIfNoForum()
		{
			var service = GetService();
			_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).Returns((Forum)null);

			Assert.Throws<Exception>(() => service.MoveForumDown(1));
		}

		[Fact]
		public void PostRestrictions()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			var roles = new List<string> {"leader", "follower"};
			_mockForumRepo.Setup(f => f.GetForumPostRoles(forum.ForumID)).Returns(roles);
			var result = service.GetForumPostRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumPostRoles(forum.ForumID), Times.Once());
			Assert.Same(roles, result);
		}

		[Fact]
		public void ViewRestrictions()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			var roles = new List<string> { "leader", "follower" };
			_mockForumRepo.Setup(f => f.GetForumViewRoles(forum.ForumID)).Returns(roles);
			var result = service.GetForumViewRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumViewRoles(forum.ForumID), Times.Once());
			Assert.Same(roles, result);
		}

		[Fact]
		public void AddPostRole()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.AddPostRole(forum, "admin");
			_mockForumRepo.Verify(f => f.AddPostRole(forum.ForumID, "admin"));
		}

		[Fact]
		public void RemovePostRole()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.RemovePostRole(forum, "admin");
			_mockForumRepo.Verify(f => f.RemovePostRole(forum.ForumID, "admin"));
		}

		[Fact]
		public void AddViewRole()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.AddViewRole(forum, "admin");
			_mockForumRepo.Verify(f => f.AddViewRole(forum.ForumID, "admin"));
		}

		[Fact]
		public void RemoveViewRole()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.RemoveViewRole(forum, "admin");
			_mockForumRepo.Verify(f => f.RemoveViewRole(forum.ForumID, "admin"));
		}

		[Fact]
		public void RemoveAllPostRoles()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.RemoveAllPostRoles(forum);
			_mockForumRepo.Verify(f => f.RemoveAllPostRoles(forum.ForumID));
		}

		[Fact]
		public void RemoveAllViewRoles()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			service.RemoveAllViewRoles(forum);
			_mockForumRepo.Verify(f => f.RemoveAllViewRoles(forum.ForumID));
		}

		[Fact]
		public void GetViewableForumIDsFromViewRestrictedForumsReturnsEmptyDictionaryWithoutUser()
		{
			var graph = new Dictionary<int, List<string>>
			{
				{1, new List<string> {"blah"}},
				{2, new List<string>()},
				{3, new List<string> {"blah"}}
			};
			var service = GetService();
			_mockForumRepo.Setup(x => x.GetAllVisible()).Returns(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
			});
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetViewableForumIDsFromViewRestrictedForums(null);
			Assert.Single(result);
			Assert.Equal(2, result[0]);
		}

		[Fact]
		public void GetViewableForumIDsFromViewRestrictedForumsDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			_mockForumRepo.Setup(x => x.GetAllVisible()).Returns(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
			});
			var result = service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new [] {"blah"}.ToList() });
			Assert.Equal(3, result.Count);
		}

		[Fact]
		public void GetViewableForumIDsFromViewRestrictedForumsReturnsIDsWithMatchingUserRoles()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blep" });
			graph.Add(4, new List<string> { "burp", "blah" });
			graph.Add(5, new List<string> { "burp" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph); _mockForumRepo.Setup(x => x.GetAllVisible()).Returns(new List<Forum>
			{
				new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }, new Forum { ForumID = 4 }, new Forum { ForumID = 5 }
			});
			var result = service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new[] { "blah", "blep" }.ToList() });
			Assert.Equal(4, result.Count);
			Assert.Contains(1, result);
			Assert.Contains(2, result);
			Assert.Contains(3, result);
			Assert.Contains(4, result);
			Assert.DoesNotContain(5, result);
		}

		[Fact]
		public void GetNonViewableDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string>()});
			Assert.Equal(2, result.Count);
			Assert.DoesNotContain(2, result);
		}

		[Fact]
		public void GetNonViewableDoesntIncludeForumsWithRoleMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Single(result);
			Assert.DoesNotContain(3, result);
		}

		[Fact]
		public void GetNonViewableIncludesForumsWithNoMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Single(result);
			Assert.Equal(1, result[0]);
		}

		[Fact]
		public void GetNonViewableExcludesViewRestrictionsForNoUser()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(null);
			Assert.Equal(2, result.Count);
			Assert.Equal(1, result[0]);
			Assert.Equal(3, result[1]);
		}

		[Fact]
		public void GetCategorizedForUserHasOnlyViewableForums()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var allForums = new List<Forum> {new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 } };
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			_mockForumRepo.Setup(f => f.GetAllVisible()).Returns(allForums);
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(new List<Category>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("whatever");
			var container = service.GetCategorizedForumContainerFilteredForUser(new User { UserID = 123, Roles = new List<string> { "OK" } });
			Assert.Equal(2, container.UncategorizedForums.Count);
			Assert.Null(container.UncategorizedForums.SingleOrDefault(f => f.ForumID == 1));
		}

		[Fact]
		public void GetCategorizedForUserPopulatesReadStatus()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(new List<Category>());
			_mockForumRepo.Setup(f => f.GetAllVisible()).Returns(new List<Forum>());
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(new Dictionary<int, List<string>>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("");
			service.GetCategorizedForumContainerFilteredForUser(user);
			_mockLastReadService.Verify(l => l.GetForumReadStatus(user, It.IsAny<CategorizedForumContainer>()), Times.Exactly(1));
		}

		[Fact]
		public void GetCategoryContainersWithForumsMapsCatsWithUnCatForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).Returns(categories);
			var forums = new List<Forum>
			{
				new Forum {ForumID = 1, CategoryID = null},
				new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
				new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
				new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
				new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
				new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
			};
			_mockForumRepo.Setup(x => x.GetAll()).Returns(forums);

			var result = service.GetCategoryContainersWithForums();

			Assert.Equal(0, result[0].Category.CategoryID);
			Assert.Equal(2, result[1].Category.CategoryID);
			Assert.Equal(3, result[2].Category.CategoryID);
			Assert.Equal(1, result[3].Category.CategoryID);
		}

		[Fact]
		public void GetCategoryContainersWithForumsMapsCatsWithoutUnCatForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).Returns(categories);
			var forums = new List<Forum>
			{
				new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
				new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
				new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
				new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
				new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
			};
			_mockForumRepo.Setup(x => x.GetAll()).Returns(forums);

			var result = service.GetCategoryContainersWithForums();

			Assert.Equal(2, result[0].Category.CategoryID);
			Assert.Equal(3, result[1].Category.CategoryID);
			Assert.Equal(1, result[2].Category.CategoryID);
		}

		[Fact]
		public void GetCategoryContainersWithForumsMapsForums()
		{
			var service = GetService();
			var categories = new List<Category>
			{
				new Category {CategoryID = 1, SortOrder = 5},
				new Category {CategoryID = 2, SortOrder = 1},
				new Category {CategoryID = 3, SortOrder = 3}
			};
			_mockCategoryRepo.Setup(x => x.GetAll()).Returns(categories);
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
			_mockForumRepo.Setup(x => x.GetAll()).Returns(forums);

			var result = service.GetCategoryContainersWithForums();

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
			public void ThrowsIfNoForumMatch()
			{
				var service = GetService();
				_mockForumRepo.Setup(x => x.Get(It.IsAny<int>())).Returns((Forum) null);

				Assert.Throws<Exception>(() => service.ModifyForumRoles(new ModifyForumRolesContainer()));
			}

			private void CallSetup(ModifyForumRolesType modifyType, out int forumID, out string role)
			{
				var service = GetService();
				var forum = new Forum { ForumID = 123 };
				forumID = forum.ForumID;
				role = "role";
				_mockForumRepo.Setup(x => x.Get(forum.ForumID)).Returns(forum);

				service.ModifyForumRoles(new ModifyForumRolesContainer { ForumID = forum.ForumID, ModifyType = modifyType, Role = role });
			}

			[Fact]
			public void AddPostCallsRepo()
			{
				CallSetup(ModifyForumRolesType.AddPost, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.AddPostRole(forumID, role), Times.Once);
			}

			[Fact]
			public void RemovePostCallsRepo()
			{
				CallSetup(ModifyForumRolesType.RemovePost, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.RemovePostRole(forumID, role), Times.Once);
			}

			[Fact]
			public void AddViewCallsRepo()
			{
				CallSetup(ModifyForumRolesType.AddView, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.AddViewRole(forumID, role), Times.Once);
			}

			[Fact]
			public void RemoveViewCallsRepo()
			{
				CallSetup(ModifyForumRolesType.RemoveView, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.RemoveViewRole(forumID, role), Times.Once);
			}

			[Fact]
			public void RemoveAllPostCallsRepo()
			{
				CallSetup(ModifyForumRolesType.RemoveAllPost, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.RemoveAllPostRoles(forumID), Times.Once);
			}

			[Fact]
			public void RemoveAllViewCallsRepo()
			{
				CallSetup(ModifyForumRolesType.RemoveAllView, out int forumID, out string role);

				_mockForumRepo.Verify(x => x.RemoveAllViewRoles(forumID), Times.Once);
			}
		}
	}
}
