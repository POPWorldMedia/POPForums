using System;
using System.Linq;
using Moq;
using NUnit.Framework;
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
	[TestFixture]
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
			return new ForumService(_mockForumRepo.Object, _mockTopicRepo.Object, _mockPostRepo.Object, _mockCategoryRepo.Object, _mockProfileRepo.Object, _mockTextParser.Object, _mockSettingsManager.Object, _mockLastReadService.Object, _eventPublisher.Object, _broker.Object);
		}

		private User DoUpNewTopic()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var forumService = GetService();
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			_mockPostRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<int>())).Returns(69);
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			forumService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			return user;
		}

		[Test]
		public void Get()
		{
			const int forumID = 123;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.Get(forumID)).Returns(new Forum(forumID));
			var forum = forumService.Get(forumID);
			Assert.AreEqual(forumID, forum.ForumID);
			_mockForumRepo.Verify(f => f.Get(forumID), Times.Once());
		}

		[Test]
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
			var forum = new Forum(forumID) {CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder};
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetAll()).Returns(new List<Forum> { new Forum(1) { SortOrder = 9 }, new Forum(2) { SortOrder = 6 }, forum});
			var result = forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter);
			Assert.AreEqual(forum, result);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(123, 0), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(2, 2), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateSortOrder(1, 4), Times.Once());
		}

		[Test]
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
			var forum = new Forum(forumID) { CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title", adapter), Times.Once());
		}

		[Test]
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
			var forum = new Forum(forumID) { CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
			_mockForumRepo.Setup(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, It.IsAny<String>(), adapter)).Returns(forum);
			_mockForumRepo.Setup(f => f.GetUrlNamesThatStartWith(title.ToUrlName())).Returns(new List<string> {"forum-title", "forum-title-but-not", "forum-title-2"});
			forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter);
			_mockForumRepo.Verify(f => f.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title-3", adapter), Times.Once());
		}

		[Test]
		public void UpdateLast()
		{
			const int forumID = 123;
			const int topicID = 456;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum(forumID);
			var topic = new Topic(topicID) { LastPostTime = lastTime, LastPostName = lastName };
			var forumService = GetService();
			_mockTopicRepo.Setup(t => t.GetLastUpdatedTopic(forum.ForumID)).Returns(topic);
			forumService.UpdateLast(forum);
			_mockTopicRepo.Verify(t => t.GetLastUpdatedTopic(forum.ForumID), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName), Times.Once());
		}

		[Test]
		public void UpdateLastWithValues()
		{
			var forumService = GetService();
			const int forumID = 123;
			var lastTime = new DateTime(2001, 2, 2);
			const string lastName = "Jeff";
			var forum = new Forum(forumID);
			forumService.UpdateLast(forum, lastTime, lastName);
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName), Times.Once());
		}

		//[Test]
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

		[Test]
		public void IncrementPostCount()
		{
			const int forumID = 123;
			var forum = new Forum(forumID);
			var forumService = GetService();
			forumService.IncrementPostCount(forum);
			_mockForumRepo.Verify(f => f.IncrementPostCount(forumID), Times.Once());
		}

		[Test]
		public void IncrementTopicAndPostCount()
		{
			const int forumID = 123;
			var forum = new Forum(forumID);
			var forumService = GetService();
			forumService.IncrementPostAndTopicCount(forum);
			_mockForumRepo.Verify(f => f.IncrementPostAndTopicCount(forumID), Times.Once());
		}

		[Test]
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
			Assert.AreEqual(container.AllForums, forums);
			Assert.AreEqual(container.AllCategories, cats);
		}

		private User GetUser()
		{
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string>();
			return user;
		}

		[Test]
		public void GetPermissionNoViewRestrictionWithUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), GetUser());
			Assert.True(permission.UserCanView);
			Assert.IsNullOrEmpty(permission.DenialReason);
		}

		[Test]
		public void GetPermissionNoViewRestrictionWithoutUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), null);
			Assert.True(permission.UserCanView);
		}

		[Test]
		public void GetPermissionViewRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>{"blah"});
			var permission = forumService.GetPermissionContext(new Forum(1), GetUser());
			Assert.False(permission.UserCanView);
		}

		[Test]
		public void GetPermissionViewRestrictionUserCantPostEither()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum(1), GetUser());
			Assert.False(permission.UserCanView);
			Assert.False(permission.UserCanPost);
		}

		[Test]
		public void GetPermissionViewRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum(1), null);
			Assert.False(permission.UserCanView);
		}

		[Test]
		public void GetPermissionViewRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum(1), user);
			Assert.True(permission.UserCanView);
		}

		[Test]
		public void GetPermissionPostRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), null);
			Assert.False(permission.UserCanPost);
		}

		[Test]
		public void GetPermissionPostRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), user);
			Assert.True(permission.UserCanPost);
			Assert.IsNullOrEmpty(permission.DenialReason);
		}

		[Test]
		public void GetPermissionPostRestrictionUserNotApproved()
		{
			var user = GetUser();
			user.IsApproved = false;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), user);
			Assert.False(permission.UserCanPost);
			Assert.IsNotNullOrEmpty(permission.DenialReason);
		}

		[Test]
		public void GetPermissionPostRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), GetUser());
			Assert.False(permission.UserCanPost);
			Assert.IsNotNullOrEmpty(permission.DenialReason);
		}

		[Test]
		public void GetPermissionModerateNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), null);
			Assert.False(permission.UserCanModerate);
		}

		[Test]
		public void GetPermissionModerateUserIsAdmin()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Admin);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), user);
			Assert.True(permission.UserCanModerate);
		}

		[Test]
		public void GetPermissionModerateUserIsModerator()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum(1), user);
			Assert.True(permission.UserCanModerate);
		}

		[Test]
		public void GetPermissionTopicClosed()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum(1), user, new Topic(4) {IsClosed = true});
			Assert.False(premission.UserCanPost);
		}

		[Test]
		public void GetPermissionTopicOpen()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum(1), user, new Topic(4) { IsClosed = false });
			Assert.True(premission.UserCanPost);
		}

		[Test]
		public void GetPermissionWithUserTopicDeleted()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum(1), user, new Topic(4) { IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Test]
		public void GetPermissionAnonTopicDeleted()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum(1), null, new Topic(4) { IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Test]
		public void GetPermissionModOnTopicDeleted()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum(1), user, new Topic(4) { IsDeleted = true });
			Assert.True(premission.UserCanView);
		}

		[Test]
		public void UserWithoutPermissionThrowsOnPost()
		{
			var topicService = GetService();
			Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum(1), GetUser(), new ForumPermissionContext { UserCanPost = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
			Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum(1), GetUser(), new ForumPermissionContext { UserCanView = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
		}

		[Test]
		public void PostNewTopicCallsTopicRepoCreate()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_mockTopicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, false, "parsed-title"), Times.Once());
		}

		[Test]
		public void PostNewTopicCallsTextParserRichText()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = false };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_mockTextParser.Verify(t => t.EscapeHtmlAndCensor("mah title"), Times.Once());
			_mockTextParser.Verify(t => t.ClientHtmlToHtml("mah text"), Times.Once());
			_mockTextParser.Verify(t => t.ForumCodeToHtml("mah text"), Times.Exactly(0));
		}

		[Test]
		public void PostNewTopicCallsTextParserPlainText()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = true };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_mockTextParser.Verify(t => t.EscapeHtmlAndCensor("mah title"), Times.Once());
			_mockTextParser.Verify(t => t.ClientHtmlToHtml("mah text"), Times.Exactly(0));
			_mockTextParser.Verify(t => t.ForumCodeToHtml("mah text"), Times.Exactly(1));
		}

		[Test]
		public void PostNewTopicCallsForumTopicPostIncrement()
		{
			DoUpNewTopic();
			_mockForumRepo.Verify(f => f.IncrementPostAndTopicCount(1), Times.Once());
		}

		[Test]
		public void PostNewTopicCallsForumUpdateLastUser()
		{
			var user = DoUpNewTopic();
			_mockForumRepo.Verify(f => f.UpdateLastTimeAndUser(1, It.IsAny<DateTime>(), user.Name), Times.Once());
		}

		[Test]
		public void PostNewTopicCallsProfileSetLastPost()
		{
			var user = DoUpNewTopic();
			_mockProfileRepo.Verify(p => p.SetLastPostID(user.UserID, 69), Times.Once());
		}

		[Test]
		public void PostNewTopicPublishesNewTopicEvent()
		{
			var user = DoUpNewTopic();
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false), Times.Once());
		}

		[Test]
		public void PostNewTopicPublishesNewPostEvent()
		{
			var user = DoUpNewTopic();
			_eventPublisher.Verify(x => x.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
		}

		[Test]
		public void PostNewTopicCallsBroker()
		{
			DoUpNewTopic();
			_broker.Verify(x => x.NotifyForumUpdate(It.IsAny<Forum>()), Times.Once());
			_broker.Verify(x => x.NotifyTopicUpdate(It.IsAny<Topic>(), It.IsAny<Forum>(), It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void PostNewTopicDoesNotPublishToFeedIfForumHasViewRestrictions()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string> { "Admin" });
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			_mockTopicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, false, "parsed-title")).Returns(2);
			var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true), Times.Once());
		}

		[Test]
		public void PostNewTopicReturnsTopic()
		{
			var forum = new Forum(1);
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var topicService = GetService();
			_mockForumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_mockTopicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_mockTextParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_mockTextParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			_mockTopicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, false, "parsed-title")).Returns(2);
			var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			Assert.AreEqual(2, topic.TopicID);
			Assert.AreEqual(forum.ForumID, topic.ForumID);
			Assert.AreEqual("parsed title", topic.Title);
			Assert.AreEqual(0, topic.ReplyCount);
			Assert.AreEqual(0, topic.ViewCount);
			Assert.AreEqual(user.UserID, topic.StartedByUserID);
			Assert.AreEqual(user.Name, topic.StartedByName);
			Assert.AreEqual(user.UserID, topic.LastPostUserID);
			Assert.AreEqual(user.Name, topic.LastPostName);
			Assert.IsFalse(topic.IsClosed);
			Assert.IsFalse(topic.IsDeleted);
			Assert.IsFalse(topic.IsIndexed);
			Assert.IsFalse(topic.IsPinned);
			Assert.AreEqual("parsed-title", topic.UrlName);
		}

		[Test]
		public void MoveUp()
		{
			var f1 = new Forum(123) { SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum(456) { SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum(789) { SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum(1000) { SortOrder = 6, CategoryID = 777 };
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
			Assert.AreEqual(0, f1.SortOrder);
			Assert.AreEqual(2, f3.SortOrder);
			Assert.AreEqual(4, f2.SortOrder);
			Assert.AreEqual(6, f4.SortOrder);
		}

		[Test]
		public void MoveDown()
		{
			var f1 = new Forum(123) { SortOrder = 0, CategoryID = 777 };
			var f2 = new Forum(456) { SortOrder = 2, CategoryID = 777 };
			var f3 = new Forum(789) { SortOrder = 4, CategoryID = 777 };
			var f4 = new Forum(1000) { SortOrder = 6, CategoryID = 777 };
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
			Assert.AreEqual(0, f1.SortOrder);
			Assert.AreEqual(2, f2.SortOrder);
			Assert.AreEqual(4, f4.SortOrder);
			Assert.AreEqual(6, f3.SortOrder);
		}

		[Test]
		public void PostRestrictions()
		{
			var service = GetService();
			var forum = new Forum(1);
			var roles = new List<string> {"leader", "follower"};
			_mockForumRepo.Setup(f => f.GetForumPostRoles(forum.ForumID)).Returns(roles);
			var result = service.GetForumPostRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumPostRoles(forum.ForumID), Times.Once());
			Assert.AreSame(roles, result);
		}

		[Test]
		public void ViewRestrictions()
		{
			var service = GetService();
			var forum = new Forum(1);
			var roles = new List<string> { "leader", "follower" };
			_mockForumRepo.Setup(f => f.GetForumViewRoles(forum.ForumID)).Returns(roles);
			var result = service.GetForumViewRoles(forum);
			_mockForumRepo.Verify(f => f.GetForumViewRoles(forum.ForumID), Times.Once());
			Assert.AreSame(roles, result);
		}

		[Test]
		public void AddPostRole()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.AddPostRole(forum, "admin");
			_mockForumRepo.Verify(f => f.AddPostRole(forum.ForumID, "admin"));
		}

		[Test]
		public void RemovePostRole()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.RemovePostRole(forum, "admin");
			_mockForumRepo.Verify(f => f.RemovePostRole(forum.ForumID, "admin"));
		}

		[Test]
		public void AddViewRole()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.AddViewRole(forum, "admin");
			_mockForumRepo.Verify(f => f.AddViewRole(forum.ForumID, "admin"));
		}

		[Test]
		public void RemoveViewRole()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.RemoveViewRole(forum, "admin");
			_mockForumRepo.Verify(f => f.RemoveViewRole(forum.ForumID, "admin"));
		}

		[Test]
		public void RemoveAllPostRoles()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.RemoveAllPostRoles(forum);
			_mockForumRepo.Verify(f => f.RemoveAllPostRoles(forum.ForumID));
		}

		[Test]
		public void RemoveAllViewRoles()
		{
			var service = GetService();
			var forum = new Forum(1);
			service.RemoveAllViewRoles(forum);
			_mockForumRepo.Verify(f => f.RemoveAllViewRoles(forum.ForumID));
		}

		[Test]
		public void GetViewableForumIDsFromViewRestrictedForumsReturnsEmptyDictionaryWithoutUser()
		{
			var service = GetService();
			var result = service.GetViewableForumIDsFromViewRestrictedForums(null);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void GetViewableForumIDsFromViewRestrictedForumsDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetViewableForumIDsFromViewRestrictedForums(new User(123, DateTime.MinValue) { Roles = new [] {"blah"}.ToList() });
			Assert.AreEqual(2, result.Count);
			Assert.False(result.Contains(2));
		}

		[Test]
		public void GetViewableForumIDsFromViewRestrictedForumsReturnsIDsWithMatchingUserRoles()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blep" });
			graph.Add(4, new List<string> { "burp", "blah" });
			graph.Add(5, new List<string> { "burp" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetViewableForumIDsFromViewRestrictedForums(new User(123, DateTime.MinValue) { Roles = new[] { "blah", "blep" }.ToList() });
			Assert.AreEqual(3, result.Count);
			Assert.False(result.Contains(2));
			Assert.False(result.Contains(5));
			Assert.True(result.Contains(1));
			Assert.True(result.Contains(3));
			Assert.True(result.Contains(4));
		}

		[Test]
		public void GetNonViewableDoesntIncludeForumsWithNoViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "blah" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User(123, DateTime.MinValue){Roles = new List<string>()});
			Assert.AreEqual(2, result.Count);
			Assert.False(result.Contains(2));
		}

		[Test]
		public void GetNonViewableDoesntIncludeForumsWithRoleMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User(123, DateTime.MinValue) { Roles = new List<string> { "OK" } });
			Assert.AreEqual(1, result.Count);
			Assert.False(result.Contains(3));
		}

		[Test]
		public void GetNonViewableIncludesForumsWithNoMatchingViewRestrictions()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(new User(123, DateTime.MinValue) { Roles = new List<string> { "OK" } });
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0]);
		}

		[Test]
		public void GetNonViewableExcludesViewRestrictionsForNoUser()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			var result = service.GetNonViewableForumIDs(null);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(1, result[0]);
			Assert.AreEqual(3, result[1]);
		}

		[Test]
		public void GetCategorizedForUserHasOnlyViewableForums()
		{
			var graph = new Dictionary<int, List<string>>();
			graph.Add(1, new List<string> { "blah" });
			graph.Add(2, new List<string>());
			graph.Add(3, new List<string> { "OK" });
			var allForums = new List<Forum> {new Forum(1), new Forum(2), new Forum(3)};
			var service = GetService();
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(graph);
			_mockForumRepo.Setup(f => f.GetAllVisible()).Returns(allForums);
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(new List<Category>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("whatever");
			var container = service.GetCategorizedForumContainerFilteredForUser(new User(123, DateTime.MinValue) { Roles = new List<string> { "OK" } });
			Assert.AreEqual(2, container.UncategorizedForums.Count);
			Assert.Null(container.UncategorizedForums.SingleOrDefault(f => f.ForumID == 1));
		}

		[Test]
		public void GetCategorizedForUserPopulatesReadStatus()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(new List<Category>());
			_mockForumRepo.Setup(f => f.GetAllVisible()).Returns(new List<Forum>());
			_mockForumRepo.Setup(f => f.GetForumViewRestrictionRoleGraph()).Returns(new Dictionary<int, List<string>>());
			_mockSettingsManager.Setup(s => s.Current.ForumTitle).Returns("");
			service.GetCategorizedForumContainerFilteredForUser(user);
			_mockLastReadService.Verify(l => l.GetForumReadStatus(user, It.IsAny<CategorizedForumContainer>()), Times.Exactly(1));
		}
	}
}
