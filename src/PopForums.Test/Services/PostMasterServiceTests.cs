using System;
using System.Collections.Generic;
using Moq;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class PostMasterServiceTests
	{
		private PostMasterService GetService()
		{
			_textParser = new Mock<ITextParsingService>();
			_topicRepo = new Mock<ITopicRepository>();
			_postRepo = new Mock<IPostRepository>();
			_forumRepo = new Mock<IForumRepository>();
			_profileRepo = new Mock<IProfileRepository>();
			_eventPublisher = new Mock<IEventPublisher>();
			_broker = new Mock<IBroker>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			return new PostMasterService(_textParser.Object, _topicRepo.Object, _postRepo.Object, _forumRepo.Object, _profileRepo.Object, _eventPublisher.Object, _broker.Object, _searchIndexQueueRepo.Object, _tenantService.Object);
		}

		private Mock<ITextParsingService> _textParser;
		private Mock<ITopicRepository> _topicRepo;
		private Mock<IPostRepository> _postRepo;
		private Mock<IForumRepository> _forumRepo;
		private Mock<IProfileRepository> _profileRepo;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<IBroker> _broker;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;
		private Mock<ITenantService> _tenantService;

		private User DoUpNewTopic()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var forumService = GetService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<int>())).Returns(69);
			_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_topicRepo.Setup(x => x.Create(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(111);
			forumService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			return user;
		}

		private User GetUser()
		{
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string>();
			return user;
		}

		public class PostNewTopicTests : PostMasterServiceTests
		{
			[Fact]
			public void UserWithoutPermissionThrowsOnPost()
			{
				var topicService = GetService();
				Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanPost = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
				Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanView = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
			}

			[Fact]
			public void CallsTopicRepoCreate()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public void CallsForumTopicPostIncrement()
			{
				DoUpNewTopic();
				_forumRepo.Verify(f => f.IncrementPostAndTopicCount(1), Times.Once());
			}

			[Fact]
			public void CallsForumUpdateLastUser()
			{
				var user = DoUpNewTopic();
				_forumRepo.Verify(f => f.UpdateLastTimeAndUser(1, It.IsAny<DateTime>(), user.Name), Times.Once());
			}

			[Fact]
			public void CallsProfileSetLastPost()
			{
				var user = DoUpNewTopic();
				_profileRepo.Verify(p => p.SetLastPostID(user.UserID, 69), Times.Once());
			}

			[Fact]
			public void PublishesNewTopicEvent()
			{
				var user = DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false), Times.Once());
			}

			[Fact]
			public void PublishesNewPostEvent()
			{
				var user = DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
			}

			[Fact]
			public void CallsBroker()
			{
				DoUpNewTopic();
				_broker.Verify(x => x.NotifyForumUpdate(It.IsAny<Forum>()), Times.Once());
				_broker.Verify(x => x.NotifyTopicUpdate(It.IsAny<Topic>(), It.IsAny<Forum>(), It.IsAny<string>()), Times.Once());
			}

			[Fact]
			public void QueuesTopicForIndexing()
			{
				DoUpNewTopic();
				_tenantService.Setup(x => x.GetTenant()).Returns("");
				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public void DoesNotPublishToFeedIfForumHasViewRestrictions()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string> { "Admin" });
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
				var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true), Times.Once());
			}

			[Fact]
			public void ReturnsTopic()
			{
				var forum = new Forum {ForumID = 1};
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost {Title = title, FullText = text, ItemID = 1};
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
				var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext {UserCanPost = true, UserCanView = true}, newPost, ip, It.IsAny<string>(), x => "");
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
		}
	}
}