using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.Configuration;
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
			_subscribedTopicsService = new Mock<ISubscribedTopicsService>();
			_moderationLogService = new Mock<IModerationLogService>();
			_forumPermissionService = new Mock<IForumPermissionService>();
			_settingsManager = new Mock<ISettingsManager>();
			_topicViewCountService = new Mock<ITopicViewCountService>();
			return new PostMasterService(_textParser.Object, _topicRepo.Object, _postRepo.Object, _forumRepo.Object, _profileRepo.Object, _eventPublisher.Object, _broker.Object, _searchIndexQueueRepo.Object, _tenantService.Object, _subscribedTopicsService.Object, _moderationLogService.Object, _forumPermissionService.Object, _settingsManager.Object, _topicViewCountService.Object);
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
		private Mock<ISubscribedTopicsService> _subscribedTopicsService;
		private Mock<IModerationLogService> _moderationLogService;
		private Mock<IForumPermissionService> _forumPermissionService;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<ITopicViewCountService> _topicViewCountService;

		private async Task<User> DoUpNewTopic()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var service = GetService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
			_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(69);
			_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
			_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
			_topicRepo.Setup(x => x.Create(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(111);
			_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext {UserCanModerate = false, UserCanPost = true, UserCanView = true});
			await service.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");
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
			public async Task NoUserReturnsFalseIsSuccess()
			{
				var service = GetService();

				var result = await service.PostNewTopic(null, new NewPost(), "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
			}

			[Fact]
			public async Task UserWithoutPostPermissionReturnsFalseIsSuccess()
			{
				var service = GetService();
				var forum = new Forum{ForumID = 1};
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				var user = GetUser();
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext {DenialReason = Resources.ForumNoPost, UserCanModerate = false, UserCanPost = false, UserCanView = true});

				var result = await service.PostNewTopic(user, new NewPost {ItemID = forum.ForumID}, "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
			}

			[Fact]
			public async Task UserWithoutViewPermissionReturnsFalseIsSuccess()
			{
				var service = GetService();
				var forum = new Forum { ForumID = 1 };
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				var user = GetUser();
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { DenialReason = Resources.ForumNoView, UserCanModerate = false, UserCanPost = false, UserCanView = false });

				var result = await service.PostNewTopic(user, new NewPost { ItemID = forum.ForumID }, "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
			}

			[Fact]
			public async Task NoForumMatchThrows()
			{
				var service = GetService();
				_forumRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum) null);

				await Assert.ThrowsAsync<Exception>(async () => await service.PostNewTopic(GetUser(), new NewPost {ItemID = 1}, "", "", x => "", x => ""));
			}

			[Fact]
			public async Task CallsPostRepoCreateWithPlainText()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = true };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("html text");
				_textParser.Setup(t => t.ForumCodeToHtml("mah text")).Returns("bb text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });

				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");

				_postRepo.Verify(x => x.Create(It.IsAny<int>(), It.IsAny<int>(), ip, true, It.IsAny<bool>(), user.UserID, user.Name, "parsed title", "bb text", It.IsAny<DateTime>(), false, user.Name, null, false, 0));
				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public async Task CallsPostRepoCreateWithHtmlText()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = false };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("html text");
				_textParser.Setup(t => t.ForumCodeToHtml("mah text")).Returns("bb text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });

				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");

				_postRepo.Verify(x => x.Create(It.IsAny<int>(), It.IsAny<int>(), ip, true, It.IsAny<bool>(), user.UserID, user.Name, "parsed title", "html text", It.IsAny<DateTime>(), false, user.Name, null, false, 0));
				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public async Task CallsPostRepoWithTopicID()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = false };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("html text");
				_textParser.Setup(t => t.ForumCodeToHtml("mah text")).Returns("bb text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });
				_topicRepo.Setup(x => x.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).ReturnsAsync(543);

				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");

				_postRepo.Verify(x => x.Create(543, It.IsAny<int>(), ip, true, It.IsAny<bool>(), user.UserID, user.Name, "parsed title", "html text", It.IsAny<DateTime>(), false, user.Name, null, false, 0));
			}

			[Fact]
			public async Task DupeOfLastPostReturnsFalseIsSuccess()
			{
				var service = GetService();
				var forum = new Forum { ForumID = 1 };
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				var user = GetUser();
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				var lastPost = "last post text";
				var lastPostID = 456;
				_profileRepo.Setup(x => x.GetLastPostID(user.UserID)).ReturnsAsync(lastPostID);
				_postRepo.Setup(x => x.Get(lastPostID)).ReturnsAsync(new Post {FullText = lastPost, PostTime = DateTime.MinValue});
				_textParser.Setup(x => x.ClientHtmlToHtml(lastPost)).Returns(lastPost);
				_settingsManager.Setup(x => x.Current.MinimumSecondsBetweenPosts).Returns(9);

				var result = await service.PostNewTopic(user, new NewPost { ItemID = forum.ForumID, FullText = lastPost }, "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(string.Format(Resources.PostWait, 9), result.Message);
			}

			[Fact]
			public async Task MinimumTimeBetweenPostsNotMetReturnsFalseIsSuccess()
			{
				var service = GetService();
				var forum = new Forum { ForumID = 1 };
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				var user = GetUser();
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				var lastPost = "last post text";
				var lastPostID = 456;
				_profileRepo.Setup(x => x.GetLastPostID(user.UserID)).ReturnsAsync(lastPostID);
				_postRepo.Setup(x => x.Get(lastPostID)).ReturnsAsync(new Post { FullText = lastPost, PostTime = DateTime.UtcNow });
				_textParser.Setup(x => x.ClientHtmlToHtml(lastPost)).Returns(lastPost);
				_settingsManager.Setup(x => x.Current.MinimumSecondsBetweenPosts).Returns(9);

				var result = await service.PostNewTopic(user, new NewPost { ItemID = forum.ForumID, FullText = "oiheorihgeorihg" }, "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(string.Format(Resources.PostWait, 9), result.Message);
			}

			[Fact]
			public async Task CallsTopicRepoCreate()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });
				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");
				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public async Task TitleIsParsed()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });

				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");

				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public async Task CallsForumTopicPostIncrement()
			{
				await DoUpNewTopic();
				_forumRepo.Verify(f => f.IncrementPostAndTopicCount(1), Times.Once());
			}

			[Fact]
			public async Task CallsForumUpdateLastUser()
			{
				var user = await DoUpNewTopic();
				_forumRepo.Verify(f => f.UpdateLastTimeAndUser(1, It.IsAny<DateTime>(), user.Name), Times.Once());
			}

			[Fact]
			public async Task CallsProfileSetLastPost()
			{
				var user = await DoUpNewTopic();
				_profileRepo.Verify(p => p.SetLastPostID(user.UserID, 69), Times.Once());
			}

			[Fact]
			public async Task PublishesNewTopicEvent()
			{
				var user = await DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false), Times.Once());
			}

			[Fact]
			public async Task PublishesNewPostEvent()
			{
				var user = await DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
			}

			[Fact]
			public async Task CallsBroker()
			{
				await DoUpNewTopic();
				_broker.Verify(x => x.NotifyForumUpdate(It.IsAny<Forum>()), Times.Once());
				_broker.Verify(x => x.NotifyTopicUpdate(It.IsAny<Topic>(), It.IsAny<Forum>(), It.IsAny<string>()), Times.Once());
			}

			[Fact]
			public async Task QueuesTopicForIndexing()
			{
				await DoUpNewTopic();
				_tenantService.Setup(x => x.GetTenant()).Returns("");
				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public async Task DoesNotPublishToFeedIfForumHasViewRestrictions()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string> { "Admin" });
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).ReturnsAsync(2);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });
				await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true), Times.Once());
			}

			[Fact]
			public async Task ReturnsTopic()
			{
				var forum = new Forum {ForumID = 1};
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost {Title = title, FullText = text, ItemID = 1};
				var topicService = GetService();
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).ReturnsAsync(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).ReturnsAsync(2);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true });
				var result = await topicService.PostNewTopic(user, newPost, ip, It.IsAny<string>(), x => "", x => "");
				Assert.Equal(2, result.Data.TopicID);
				Assert.Equal(forum.ForumID, result.Data.ForumID);
				Assert.Equal("parsed title", result.Data.Title);
				Assert.Equal(0, result.Data.ReplyCount);
				Assert.Equal(0, result.Data.ViewCount);
				Assert.Equal(user.UserID, result.Data.StartedByUserID);
				Assert.Equal(user.Name, result.Data.StartedByName);
				Assert.Equal(user.UserID, result.Data.LastPostUserID);
				Assert.Equal(user.Name, result.Data.LastPostName);
				Assert.False(result.Data.IsClosed);
				Assert.False(result.Data.IsDeleted);
				Assert.False(result.Data.IsPinned);
				Assert.Equal("parsed-title", result.Data.UrlName);
			}
		}

		public class PostReplyTests : PostNewTopicTests
		{
			[Fact]
			public async Task NoUserReturnsFalseIsSuccessful()
			{
				var service = GetService();

				var result = await service.PostReply(null, 0, "", false, new NewPost(), DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
			}

			[Fact]
			public async Task NoTopicReturnsFalseIsSuccessful()
			{
				var service = GetService();
				_topicRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Topic) null);

				var result = await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.TopicNotExist, result.Message);
			}

			[Fact]
			public async Task NoForumThrows()
			{
				var service = GetService();
				_topicRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync(new Topic());
				_forumRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum) null);

				await Assert.ThrowsAsync<Exception>(async () => await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => ""));
			}

			[Fact]
			public async Task NoViewPermissionReturnsFalseIsSuccessful()
			{
				var service = GetService();
				var user = GetUser();
				var forum = new Forum {ForumID = 1};
				var topic = new Topic {ForumID = forum.ForumID};
				var newPost = new NewPost {ItemID = topic.TopicID};
				_topicRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync(topic);
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext {UserCanView = false, UserCanPost = true});

				var result = await service.PostReply(user, 0, "", false, newPost, DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.ForumNoView, result.Message);
			}

			[Fact]
			public async Task NoPostPermissionReturnsFalseIsSuccessful()
			{
				var service = GetService();
				var user = GetUser();
				var forum = new Forum { ForumID = 1 };
				var topic = new Topic { ForumID = forum.ForumID };
				var newPost = new NewPost { ItemID = topic.TopicID };
				_topicRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync(topic);
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanView = true, UserCanPost = false });

				var result = await service.PostReply(user, 0, "", false, newPost, DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.ForumNoPost, result.Message);
			}

			[Fact]
			public async Task ClosedTopicReturnsFalseIsSuccessful()
			{
				var service = GetService();
				_topicRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync(new Topic{IsClosed = true});

				var result = await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue, x => "", (u, t) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.Closed, result.Message);
			}

			[Fact]
			public async Task UsesPlainTextParsed()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ForumCodeToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = true };
				_textParser.Setup(t => t.Censor(newPost.Title)).Returns("parsed title");
				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");
				_postRepo.Verify(p => p.Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0));
			}

			[Fact]
			public async Task UsesRichTextParsed()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };
				_textParser.Setup(t => t.Censor(newPost.Title)).Returns("parsed title");
				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");
				_postRepo.Verify(p => p.Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0));
			}

			[Fact]
			public async Task DupeOfLastPostFails()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				_profileRepo.Setup(x => x.GetLastPostID(user.UserID)).ReturnsAsync(654);
				_postRepo.Setup(x => x.Get(654)).ReturnsAsync(new Post {FullText = "parsed text", PostTime = DateTime.MinValue});
				_settingsManager.Setup(x => x.Current.MinimumSecondsBetweenPosts).Returns(9);
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

				var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(string.Format(Resources.PostWait, 9), result.Message);
			}

			[Fact]
			public async Task MinTimeSinceLastPostTooShortFails()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("oihf text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				_profileRepo.Setup(x => x.GetLastPostID(user.UserID)).ReturnsAsync(654);
				_postRepo.Setup(x => x.Get(654)).ReturnsAsync(new Post { FullText = "parsed text", PostTime = DateTime.UtcNow });
				_settingsManager.Setup(x => x.Current.MinimumSecondsBetweenPosts).Returns(9);
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

				var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(string.Format(Resources.PostWait, 9), result.Message);
			}

			[Fact]
			public async Task EmptyPostFails()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				_profileRepo.Setup(x => x.GetLastPostID(user.UserID)).ReturnsAsync(654);
				_settingsManager.Setup(x => x.Current.MinimumSecondsBetweenPosts).Returns(9);
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

				var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.PostEmpty, result.Message);
			}

			[Fact]
			public async Task HitsRepo()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };
				_textParser.Setup(t => t.Censor(newPost.Title)).Returns("parsed title");

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t,p) => "", "", x => "", x => "");

				_postRepo.Verify(p => p.Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0));
			}

			[Fact]
			public async Task HitsSubscribedService()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t,p) => "", "", x => "", x => "");

				_subscribedTopicsService.Verify(s => s.NotifySubscribers(topic, user, It.IsAny<string>(), It.IsAny<Func<User, Topic, string>>()), Times.Once());
			}

			[Fact]
			public async Task IncrementsTopicReplyCount()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t,p) => "", "", x => "", x => "");

				_topicRepo.Verify(t => t.IncrementReplyCount(1));
			}

			[Fact]
			public async Task IncrementsForumPostCount()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_forumRepo.Verify(f => f.IncrementPostCount(2));
			}

			[Fact]
			public async Task UpdatesTopicLastInfo()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_topicRepo.Verify(t => t.UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime));
			}

			[Fact]
			public async Task UpdatesForumLastInfo()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_forumRepo.Verify(f => f.UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name));
			}

			[Fact]
			public async Task PostQueuesMarksTopicForIndexing()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum {ForumID = topic.ForumID};
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext {UserCanPost = true, UserCanView = true});
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				_tenantService.Setup(x => x.GetTenant()).Returns("");
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public async Task NotifiesBroker()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.Get(topic.ForumID)).ReturnsAsync(forum);
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_broker.Verify(x => x.NotifyForumUpdate(forum), Times.Once());
				_broker.Verify(x => x.NotifyTopicUpdate(topic, forum, It.IsAny<string>()), Times.Once());
				_broker.Verify(x => x.NotifyNewPost(topic, It.IsAny<int>()), Times.Once());
			}

			[Fact]
			public async Task SetsProfileLastPostID()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_profileRepo.Verify(p => p.SetLastPostID(user.UserID, result.Data.PostID), Times.Once);
			}

			[Fact]
			public async Task PublishesEvent()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, false), Times.Once());
			}

			[Fact]
			public async Task DoesNotPublishEventOnViewRestrictedForum()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				var forum = new Forum {ForumID = topic.ForumID};
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string> { "Admin" });
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
			}

			[Fact]
			public async Task ReturnsHydratedObject()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
				_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), false, true, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), false, It.IsAny<string>(), null, false, 0)).ReturnsAsync(123);
				var forum = new Forum { ForumID = topic.ForumID };
				_topicRepo.Setup(x => x.Get(topic.TopicID)).ReturnsAsync(topic);
				_forumPermissionService.Setup(x => x.GetPermissionContext(forum, user)).ReturnsAsync(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
				_textParser.Setup(x => x.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed text");
				_forumRepo.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

				var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, x => "", (t, p) => "", "", x => "", x => "");

				Assert.Equal(topic.TopicID, result.Data.TopicID);
				Assert.Equal("parsed text", result.Data.FullText);
				Assert.Equal("127.0.0.1", result.Data.IP);
				Assert.False(result.Data.IsDeleted);
				Assert.False(result.Data.IsEdited);
				Assert.False(result.Data.IsFirstInTopic);
				Assert.Equal(user.Name, result.Data.LastEditName);
				Assert.Null(result.Data.LastEditTime);
				Assert.Equal(user.Name, result.Data.Name);
				Assert.Equal(0, result.Data.ParentPostID);
				Assert.Equal(123, result.Data.PostID);
				Assert.Equal(postTime, result.Data.PostTime);
				Assert.True(result.Data.ShowSig);
				Assert.Equal("parsed title", result.Data.Title);
				Assert.Equal(user.UserID, result.Data.UserID);
			}
		}

		public class EditPostTests : PostMasterServiceTests
		{
			private new User GetUser()
			{
				return new User {UserID = 123, Roles = new List<string>()};
			}

			[Fact]
			public async Task FailsBecauseNoUserMatch()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post {UserID = 789});

				var result = await service.EditPost(456, new PostEdit(), new User {UserID = 111, Roles = new List<string>()}, x => "");

				Assert.False(result.IsSuccessful);
				Assert.Equal(Resources.Forbidden, result.Message);
			}

			[Fact]
			public async Task CensorsTitle()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123 });
				await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");
				_textParser.Verify(t => t.Censor("blah"), Times.Exactly(1));
			}

			[Fact]
			public async Task NoTitleUpdateWhenNotFirstPostInTopic()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123, IsFirstInTopic = false, Title = "blah" });
				_textParser.Setup(x => x.Censor("blah")).Returns("changed");

				await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

				_topicRepo.Verify(x => x.UpdateTitleAndForum(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			}

			[Fact]
			public async Task NoTitleUpdateWhenTitleHasNotChanged()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true, Title = "blah" });
				_textParser.Setup(x => x.Censor("blah")).Returns("blah");

				await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

				_topicRepo.Verify(x => x.UpdateTitleAndForum(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			}

			[Fact]
			public async Task NoEditWhenTitleIsEmpty()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true, Title = "blah" });
				_textParser.Setup(x => x.Censor("blah")).Returns("");

				var result = await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

				_topicRepo.Verify(x => x.UpdateTitleAndForum(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
				Assert.False(result.IsSuccessful);
			}

			[Fact]
			public async Task TitleUpdateWhenFirstPostInTopicAndTitleChanged()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, TopicID = 222, UserID = 123, IsFirstInTopic = true, Title = "blah" });
				_topicRepo.Setup(x => x.Get(222)).ReturnsAsync(new Topic {TopicID = 222, ForumID = 111});
				_forumRepo.Setup(x => x.Get(111)).ReturnsAsync(new Forum {ForumID = 111});
				_textParser.Setup(x => x.Censor("blah")).Returns("changed");
				_topicRepo.Setup(x => x.GetUrlNamesThatStartWith("changed")).ReturnsAsync(new List<string>());

				await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

				_topicRepo.Verify(x => x.UpdateTitleAndForum(222, 111, "changed", "changed"), Times.Once);
			}

			[Fact]
			public async Task PlainTextParsed()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123 });
				await service.EditPost(456, new PostEdit { FullText = "blah", IsPlainText = true }, GetUser(), x => "");
				_textParser.Verify(t => t.ForumCodeToHtml("blah"), Times.Exactly(1));
			}

			[Fact]
			public async Task RichTextParsed()
			{
				var service = GetService();
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123 });

				await service.EditPost(456, new PostEdit { FullText = "blah", IsPlainText = false }, GetUser(), x => "");

				_textParser.Verify(t => t.ClientHtmlToHtml("blah"), Times.Exactly(1));
			}

			[Fact]
			public async Task SavesMappedValues()
			{
				var service = GetService();
				var post = new Post { PostID = 67, IsFirstInTopic = true };
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true });
				_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => post = p);
				_topicRepo.Setup(x => x.Get(post.TopicID)).ReturnsAsync(new Topic {ForumID = 333});
				_forumRepo.Setup(x => x.Get(333)).ReturnsAsync(new Forum {ForumID = 333});
				_topicRepo.Setup(x => x.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
				_textParser.Setup(t => t.Censor("unparsed title")).Returns("new title");

				var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, IsFirstInTopic = true }, new User { UserID = 123, Name = "dude", Roles = new List<string>()}, x => "");

				Assert.True(result.IsSuccessful);
				Assert.NotEqual(post.LastEditTime, new DateTime(2009, 1, 1));
				Assert.Equal(456, post.PostID);
				Assert.Equal("new", post.FullText);
				Assert.Equal("new title", post.Title);
				Assert.True(post.ShowSig);
				Assert.True(post.IsEdited);
				Assert.Equal("dude", post.LastEditName);
			}

			[Fact]
			public async Task ModeratorLogged()
			{
				var service = GetService();
				var user = new User { UserID = 123, Name = "dude", Roles = new List<string>()};
				_textParser.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
				_textParser.Setup(t => t.Censor("unparsed title")).Returns("new title");
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post{PostID = 456, UserID = user.UserID, FullText = "old text"});

				var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user, x => "");

				Assert.True(result.IsSuccessful);
				_moderationLogService.Verify(m => m.LogPost(user, ModerationType.PostEdit, It.IsAny<Post>(), "mah comment", "old text"), Times.Exactly(1));
			}

			[Fact]
			public async Task QueuesTopicForIndexing()
			{
				var service = GetService();
				var user = new User { UserID = 123, Name = "dude", Roles = new List<string>() };
				var post = new Post { PostID = 456, ShowSig = false, FullText = "old text", TopicID = 999, UserID = user.UserID };
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(post);
				_tenantService.Setup(x => x.GetTenant()).Returns("");

				var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user, x => "");

				Assert.True(result.IsSuccessful);
				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public async Task ModeratorCanEdit()
			{
				var service = GetService();
				var post = new Post { PostID = 67 };
				_postRepo.Setup(x => x.Get(456)).ReturnsAsync(new Post { PostID = 456, UserID = 123 });
				_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => post = p);
				_textParser.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
				_textParser.Setup(t => t.Censor("unparsed title")).Returns("new title");

				var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true }, new User { UserID = 123, Name = "dude", Roles = new List<string>(new []{PermanentRoles.Moderator})}, x => "");

				Assert.True(result.IsSuccessful);
				_postRepo.Verify(x => x.Update(It.IsAny<Post>()), Times.Once);
			}
		}
	}
}