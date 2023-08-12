namespace PopForums.Test.Services;

public class PostMasterServiceTests
{
	private PostMasterService GetService()
	{
		_textParser = Substitute.For<ITextParsingService>();
		_topicRepo = Substitute.For<ITopicRepository>();
		_postRepo = Substitute.For<IPostRepository>();
		_forumRepo = Substitute.For<IForumRepository>();
		_profileRepo = Substitute.For<IProfileRepository>();
		_eventPublisher = Substitute.For<IEventPublisher>();
		_broker = Substitute.For<IBroker>();
		_searchIndexQueueRepo = Substitute.For<ISearchIndexQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		_subscribedTopicsService = Substitute.For<ISubscribedTopicsService>();
		_moderationLogService = Substitute.For<IModerationLogService>();
		_forumPermissionService = Substitute.For<IForumPermissionService>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_topicViewCountService = Substitute.For<ITopicViewCountService>();
		_postImageService = Substitute.For<IPostImageService>();
		return new PostMasterService(_textParser, _topicRepo, _postRepo, _forumRepo, _profileRepo, _eventPublisher, _broker, _searchIndexQueueRepo, _tenantService, _subscribedTopicsService, _moderationLogService, _forumPermissionService, _settingsManager, _topicViewCountService, _postImageService);
	}

	private ITextParsingService _textParser;
	private ITopicRepository _topicRepo;
	private IPostRepository _postRepo;
	private IForumRepository _forumRepo;
	private IProfileRepository _profileRepo;
	private IEventPublisher _eventPublisher;
	private IBroker _broker;
	private ISearchIndexQueueRepository _searchIndexQueueRepo;
	private ITenantService _tenantService;
	private ISubscribedTopicsService _subscribedTopicsService;
	private IModerationLogService _moderationLogService;
	private IForumPermissionService _forumPermissionService;
	private ISettingsManager _settingsManager;
	private ITopicViewCountService _topicViewCountService;
	private IPostImageService _postImageService;

	private async Task<User> DoUpNewTopic()
	{
		var forum = new Forum { ForumID = 1 };
		var user = GetUser();
		const string ip = "127.0.0.1";
		const string title = "mah title";
		const string text = "mah text";
		var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
		var service = GetService();
		_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
		_textParser.ClientHtmlToHtml("mah text").Returns("parsed text");
		_textParser.Censor("mah title").Returns("parsed title");
		_postRepo.Create(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<string>(), null, Arg.Any<bool>(), Arg.Any<int>()).Returns(Task.FromResult(69));
		_forumRepo.Get(forum.ForumID).Returns(forum);
		_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
		_topicRepo.Create(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>()).Returns(Task.FromResult(111));
		_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext {UserCanModerate = false, UserCanPost = true, UserCanView = true}));
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
		await service.PostNewTopic(user, newPost, ip, default, x => "", x => "");
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			var user = GetUser();
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext {DenialReason = Resources.ForumNoPost, UserCanModerate = false, UserCanPost = false, UserCanView = true}));

			var result = await service.PostNewTopic(user, new NewPost {ItemID = forum.ForumID}, "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task UserWithoutViewPermissionReturnsFalseIsSuccess()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			var user = GetUser();
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { DenialReason = Resources.ForumNoView, UserCanModerate = false, UserCanPost = false, UserCanView = false }));

			var result = await service.PostNewTopic(user, new NewPost { ItemID = forum.ForumID }, "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task NoForumMatchThrows()
		{
			var service = GetService();
			_forumRepo.Get(Arg.Any<int>()).Returns((Forum) null);

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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("html text");
			_textParser.ForumCodeToHtml("mah text").Returns("bb text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _postRepo.Received().Create(Arg.Any<int>(), Arg.Any<int>(), ip, true, Arg.Any<bool>(), user.UserID, user.Name, "parsed title", "bb text", Arg.Any<DateTime>(), false, user.Name, null, false, 0);
			await _topicRepo.Received().Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title");
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("html text");
			_textParser.ForumCodeToHtml("mah text").Returns("bb text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _postRepo.Received().Create(Arg.Any<int>(), Arg.Any<int>(), ip, true, Arg.Any<bool>(), user.UserID, user.Name, "parsed title", "html text", Arg.Any<DateTime>(), false, user.Name, null, false, 0);
			await _topicRepo.Received().Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title");
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("html text");
			_textParser.ForumCodeToHtml("mah text").Returns("bb text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_topicRepo.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title").Returns(Task.FromResult(543));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _postRepo.Received().Create(543, Arg.Any<int>(), ip, true, Arg.Any<bool>(), user.UserID, user.Name, "parsed title", "html text", Arg.Any<DateTime>(), false, user.Name, null, false, 0);
		}

		[Fact]
		public async Task CallsSubscribeServiceWithUserAndTopicIfEnabled()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = false };
			var profile = new Profile {UserID = user.UserID, IsAutoFollowOnReply = true};
			var topicService = GetService();
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("html text");
			_textParser.ForumCodeToHtml("mah text").Returns("bb text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_topicRepo.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title").Returns(Task.FromResult(543));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");
			
			await _subscribedTopicsService.Received().AddSubscribedTopic(user.UserID, 543);
		}

		[Fact]
		public async Task CallsPostImageServiceWithIDs()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var postImageIDs = new string[] {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1, IsPlainText = false, PostImageIDs = postImageIDs};
			var profile = new Profile { UserID = user.UserID, IsAutoFollowOnReply = true };
			var topicService = GetService();
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("html text");
			_textParser.ForumCodeToHtml("mah text").Returns("bb text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_topicRepo.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title").Returns(Task.FromResult(543));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _postImageService.Received().DeleteTempRecords(postImageIDs, newPost.FullText);
		}

		[Fact]
		public async Task DupeOfLastPostReturnsFalseIsSuccess()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			var user = GetUser();
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			var lastPost = "last post text";
			var lastPostID = 456;
			_profileRepo.GetLastPostID(user.UserID).Returns(lastPostID);
			_postRepo.Get(lastPostID).Returns(Task.FromResult(new Post {FullText = lastPost, PostTime = DateTime.MinValue}));
			_textParser.ClientHtmlToHtml(lastPost).Returns(lastPost);
			_settingsManager.Current.MinimumSecondsBetweenPosts.Returns(9);

			var result = await service.PostNewTopic(user, new NewPost { ItemID = forum.ForumID, FullText = lastPost }, "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
			Assert.Equal(string.Format(Resources.PostWait, 9), result.Message);
		}

		[Fact]
		public async Task MinimumTimeBetweenPostsNotMetReturnsFalseIsSuccess()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1 };
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			var user = GetUser();
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			var lastPost = "last post text";
			var lastPostID = 456;
			_profileRepo.GetLastPostID(user.UserID).Returns(lastPostID);
			_postRepo.Get(lastPostID).Returns(Task.FromResult(new Post { FullText = lastPost, PostTime = DateTime.UtcNow }));
			_textParser.ClientHtmlToHtml(lastPost).Returns(lastPost);
			_settingsManager.Current.MinimumSecondsBetweenPosts.Returns(9);

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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("parsed text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _topicRepo.Received().Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title");
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("parsed text");
			_textParser.Censor("mah title").Returns("parsed title");
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, _ => "", _ => "");

			await _topicRepo.Received().Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title");
		}

		[Fact]
		public async Task CallsForumTopicPostIncrement()
		{
			await DoUpNewTopic();
			await _forumRepo.Received().IncrementPostAndTopicCount(1);
		}

		[Fact]
		public async Task CallsForumUpdateLastUser()
		{
			var user = await DoUpNewTopic();
			await _forumRepo.Received().UpdateLastTimeAndUser(1, Arg.Any<DateTime>(), user.Name);
		}

		[Fact]
		public async Task CallsProfileSetLastPost()
		{
			var user = await DoUpNewTopic();
			await _profileRepo.Received().SetLastPostID(user.UserID, 69);
		}

		[Fact]
		public async Task PublishesNewTopicEvent()
		{
			var user = await DoUpNewTopic();
			await _eventPublisher.Received().ProcessEvent(Arg.Any<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false);
		}

		[Fact]
		public async Task PublishesNewPostEvent()
		{
			var user = await DoUpNewTopic();
			await _eventPublisher.Received().ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true);
		}

		[Fact]
		public async Task CallsBroker()
		{
			await DoUpNewTopic();
			_broker.Received().NotifyForumUpdate(Arg.Any<Forum>());
			_broker.Received().NotifyTopicUpdate(Arg.Any<Topic>(), Arg.Any<Forum>(), Arg.Any<string>());
		}

		[Fact]
		public async Task QueuesTopicForIndexing()
		{
			await DoUpNewTopic();
			_tenantService.GetTenant().Returns("");
			await _searchIndexQueueRepo.Received().Enqueue(Arg.Any<SearchIndexPayload>());
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string> { "Admin" }));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("parsed text");
			_textParser.Censor("mah title").Returns("parsed title");
			_topicRepo.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title").Returns(Task.FromResult(2));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

			await _eventPublisher.Received().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true);
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
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(new List<string>()));
			_topicRepo.GetUrlNamesThatStartWith("parsed-title").Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("mah text").Returns("parsed text");
			_textParser.Censor("mah title").Returns("parsed title");
			_topicRepo.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, Arg.Any<DateTime>(), false, false, false, "parsed-title").Returns(Task.FromResult(2));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanModerate = false, UserCanPost = true, UserCanView = true }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			var result = await topicService.PostNewTopic(user, newPost, ip, default, x => "", x => "");

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

	public class PostReplyTests : PostMasterServiceTests
	{
		[Fact]
		public async Task NoUserReturnsFalseIsSuccessful()
		{
			var service = GetService();

			var result = await service.PostReply(null, 0, "", false, new NewPost(), DateTime.MaxValue, (t) => "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task NoTopicReturnsFalseIsSuccessful()
		{
			var service = GetService();
			_topicRepo.Get(Arg.Any<int>()).Returns((Topic) null);

			var result = await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue, (t) => "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
			Assert.Equal(Resources.TopicNotExist, result.Message);
		}

		[Fact]
		public async Task NoForumThrows()
		{
			var service = GetService();
			_topicRepo.Get(Arg.Any<int>()).Returns(Task.FromResult(new Topic()));
			_forumRepo.Get(Arg.Any<int>()).Returns((Forum) null);

			await Assert.ThrowsAsync<Exception>(async () => await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue, (t) => "", "", x => "", x => ""));
		}

		[Fact]
		public async Task NoViewPermissionReturnsFalseIsSuccessful()
		{
			var service = GetService();
			var user = GetUser();
			var forum = new Forum {ForumID = 1};
			var topic = new Topic {ForumID = forum.ForumID};
			var newPost = new NewPost {ItemID = topic.TopicID};
			_topicRepo.Get(Arg.Any<int>()).Returns(Task.FromResult(topic));
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext {UserCanView = false, UserCanPost = true}));

			var result = await service.PostReply(user, 0, "", false, newPost, DateTime.MaxValue, (t) => "", "", x => "", x => "");

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
			_topicRepo.Get(Arg.Any<int>()).Returns(Task.FromResult(topic));
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanView = true, UserCanPost = false }));

			var result = await service.PostReply(user, 0, "", false, newPost, DateTime.MaxValue, (t) => "", "", x => "", x => "");

			Assert.False(result.IsSuccessful);
			Assert.Equal(Resources.ForumNoPost, result.Message);
		}

		[Fact]
		public async Task ClosedTopicReturnsFalseIsSuccessful()
		{
			var service = GetService();
			_topicRepo.Get(Arg.Any<int>()).Returns(Task.FromResult(new Topic{IsClosed = true}));

			var result = await service.PostReply(GetUser(), 0, "", false, new NewPost(), DateTime.MaxValue,(t) => "", "", x => "", x => "");

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
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ForumCodeToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = true };
			_textParser.Censor(newPost.Title).Returns("parsed title");
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _postRepo.Received().Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0);
		}

		[Fact]
		public async Task UsesRichTextParsed()
		{
			var topic = new Topic { TopicID = 1, Title = "" };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };
			_textParser.Censor(newPost.Title).Returns("parsed title");

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _postRepo.Received().Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0);
		}

		[Fact]
		public async Task DupeOfLastPostFails()
		{
			var topic = new Topic { TopicID = 1, Title = "" };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetLastPostID(user.UserID).Returns(654);
			_postRepo.Get(654).Returns(Task.FromResult(new Post {FullText = "parsed text", PostTime = DateTime.MinValue}));
			_settingsManager.Current.MinimumSecondsBetweenPosts.Returns(9);
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

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
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("oihf text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetLastPostID(user.UserID).Returns(654);
			_postRepo.Get(654).Returns(Task.FromResult(new Post { FullText = "parsed text", PostTime = DateTime.UtcNow }));
			_settingsManager.Current.MinimumSecondsBetweenPosts.Returns(9);
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

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
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetLastPostID(user.UserID).Returns(654);
			_settingsManager.Current.MinimumSecondsBetweenPosts.Returns(9);
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, IsPlainText = false };

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime,  (t) => "", "", x => "", x => "");

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
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };
			_textParser.Censor(newPost.Title).Returns("parsed title");
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime,  (_) => "", "", x => "", x => "");

			await _postRepo.Received().Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0);
		}

		[Fact]
		public async Task HitsSubscribedService()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			var tenandID = "cb";
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };
			_tenantService.GetTenant().Returns(tenandID);

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _subscribedTopicsService.Received().NotifySubscribers(topic, user, tenandID);
		}

		[Fact]
		public async Task HitsSubscribeAddWhenProfileCallsForIt()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			var profile = new Profile {UserID = user.UserID, IsAutoFollowOnReply = true};
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _subscribedTopicsService.Received().AddSubscribedTopic(user.UserID, topic.TopicID);
		}

		[Fact]
		public async Task IncrementsTopicReplyCount()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _topicRepo.Received().IncrementReplyCount(1);
		}

		[Fact]
		public async Task IncrementsForumPostCount()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _forumRepo.Received().IncrementPostCount(2);
		}

		[Fact]
		public async Task UpdatesTopicLastInfo()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _topicRepo.Received().UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime);
		}

		[Fact]
		public async Task UpdatesForumLastInfo()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _forumRepo.Received().UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name);
		}

		[Fact]
		public async Task PostQueuesMarksTopicForIndexing()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum {ForumID = topic.ForumID};
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext {UserCanPost = true, UserCanView = true}));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_tenantService.GetTenant().Returns("");
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime,(t) => "", "", x => "", x => "");

			await _searchIndexQueueRepo.Received().Enqueue(Arg.Any<SearchIndexPayload>());
		}

		[Fact]
		public async Task NotifiesBroker()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.Get(topic.ForumID).Returns(Task.FromResult(forum));
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			_broker.Received().NotifyForumUpdate(forum);
			_broker.Received().NotifyTopicUpdate(topic, forum, Arg.Any<string>());
			_broker.Received().NotifyNewPost(topic, Arg.Any<int>());
		}

		[Fact]
		public async Task SetsProfileLastPostID()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _profileRepo.Received().SetLastPostID(user.UserID, result.Data.PostID);
		}

		[Fact]
		public async Task CallsPostImageServiceWithIDs()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			var postImageIDs = new string[] {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID, PostImageIDs = postImageIDs};

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _postImageService.Received().DeleteTempRecords(postImageIDs, newPost.FullText);
		}

		[Fact]
		public async Task PublishesEvent()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime,  (t) => "", "", x => "", x => "");

			await _eventPublisher.Received().ProcessEvent(Arg.Any<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, false);
		}

		[Fact]
		public async Task DoesNotPublishEventOnViewRestrictedForum()
		{
			var topic = new Topic { TopicID = 1, ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			var forum = new Forum {ForumID = topic.ForumID};
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string> { "Admin" }));
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

			await _eventPublisher.Received().ProcessEvent(Arg.Any<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, true);
		}

		[Fact]
		public async Task ReturnsHydratedObject()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var service = GetService();
			_forumRepo.GetForumViewRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
			_postRepo.Create(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), false, true, Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), false, Arg.Any<string>(), null, false, 0).Returns(Task.FromResult(123));
			var forum = new Forum { ForumID = topic.ForumID };
			_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(topic));
			_forumPermissionService.GetPermissionContext(forum, user).Returns(Task.FromResult(new ForumPermissionContext { UserCanPost = true, UserCanView = true }));
			_textParser.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed text");
			_forumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			_textParser.Censor("mah title").Returns("parsed title");
			_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile()));
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, ItemID = topic.TopicID };

			var result = await service.PostReply(user, 0, "127.0.0.1", false, newPost, postTime, (t) => "", "", x => "", x => "");

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
			_postRepo.Get(456).Returns(Task.FromResult(new Post {UserID = 789}));

			var result = await service.EditPost(456, new PostEdit(), new User {UserID = 111, Roles = new List<string>()}, x => "");

			Assert.False(result.IsSuccessful);
			Assert.Equal(Resources.Forbidden, result.Message);
		}

		[Fact]
		public async Task CensorsTitle()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123 }));
			await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");
			_textParser.Received(1).Censor("blah");
		}

		[Fact]
		public async Task NoTitleUpdateWhenNotFirstPostInTopic()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123, IsFirstInTopic = false, Title = "blah" }));
			_textParser.Censor("blah").Returns("changed");

			await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

			await _topicRepo.DidNotReceive().UpdateTitleAndForum(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task NoTitleUpdateWhenTitleHasNotChanged()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true, Title = "blah" }));
			_textParser.Censor("blah").Returns("blah");

			await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

			await _topicRepo.DidNotReceive().UpdateTitleAndForum(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task NoEditWhenTitleIsEmpty()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true, Title = "blah" }));
			_textParser.Censor("blah").Returns("");

			var result = await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

			await _topicRepo.DidNotReceive().UpdateTitleAndForum(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task TitleUpdateWhenFirstPostInTopicAndTitleChanged()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, TopicID = 222, UserID = 123, IsFirstInTopic = true, Title = "blah" }));
			_topicRepo.Get(222).Returns(Task.FromResult(new Topic {TopicID = 222, ForumID = 111}));
			_forumRepo.Get(111).Returns(Task.FromResult(new Forum {ForumID = 111}));
			_textParser.Censor("blah").Returns("changed");
			_topicRepo.GetUrlNamesThatStartWith("changed").Returns(Task.FromResult(new List<string>()));

			await service.EditPost(456, new PostEdit { Title = "blah" }, GetUser(), x => "");

			await _topicRepo.Received().UpdateTitleAndForum(222, 111, "changed", "changed");
		}

		[Fact]
		public async Task PlainTextParsed()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123 }));
			await service.EditPost(456, new PostEdit { FullText = "blah", IsPlainText = true }, GetUser(), x => "");
			_textParser.Received(1).ForumCodeToHtml("blah");
		}

		[Fact]
		public async Task RichTextParsed()
		{
			var service = GetService();
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123 }));

			await service.EditPost(456, new PostEdit { FullText = "blah", IsPlainText = false }, GetUser(), x => "");

			_textParser.Received(1).ClientHtmlToHtml("blah");
		}

		[Fact]
		public async Task SavesMappedValues()
		{
			var service = GetService();
			var post = new Post { PostID = 67, IsFirstInTopic = true };
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123, IsFirstInTopic = true }));
			await _postRepo.Update(Arg.Do<Post>(p => post = p));
			_topicRepo.Get(post.TopicID).Returns(Task.FromResult(new Topic {ForumID = 333}));
			_forumRepo.Get(333).Returns(Task.FromResult(new Forum {ForumID = 333}));
			_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
			_textParser.ClientHtmlToHtml("blah").Returns("new");
			_textParser.Censor("unparsed title").Returns("new title");

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
			_textParser.ClientHtmlToHtml("blah").Returns("new");
			_textParser.Censor("unparsed title").Returns("new title");
			_postRepo.Get(456).Returns(Task.FromResult(new Post{PostID = 456, UserID = user.UserID, FullText = "old text"}));

			var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user, x => "");

			Assert.True(result.IsSuccessful);
			await _moderationLogService.Received(1).LogPost(user, ModerationType.PostEdit, Arg.Any<Post>(), "mah comment", "old text");
		}

		[Fact]
		public async Task QueuesTopicForIndexing()
		{
			var service = GetService();
			var user = new User { UserID = 123, Name = "dude", Roles = new List<string>() };
			var post = new Post { PostID = 456, ShowSig = false, FullText = "old text", TopicID = 999, UserID = user.UserID };
			_postRepo.Get(456).Returns(Task.FromResult(post));
			_tenantService.GetTenant().Returns("");

			var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user, x => "");

			Assert.True(result.IsSuccessful);
			await _searchIndexQueueRepo.Received().Enqueue(Arg.Any<SearchIndexPayload>());
		}

		[Fact]
		public async Task ModeratorCanEdit()
		{
			var service = GetService();
			var post = new Post { PostID = 67 };
			_postRepo.Get(456).Returns(Task.FromResult(new Post { PostID = 456, UserID = 123 }));
			await _postRepo.Update(Arg.Do<Post>(x => post = x));
			_textParser.ClientHtmlToHtml("blah").Returns("new");
			_textParser.Censor("unparsed title").Returns("new title");

			var result = await service.EditPost(456, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true }, new User { UserID = 123, Name = "dude", Roles = new List<string>(new []{PermanentRoles.Moderator})}, x => "");

			Assert.True(result.IsSuccessful);
			await _postRepo.Received().Update(Arg.Any<Post>());
		}
	}
}