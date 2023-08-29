namespace PopForums.Test.Services;

public class TopicServiceTests
{
	private ISettingsManager _settingsManager;
	private ITopicRepository _topicRepo;
	private IPostRepository _postRepo;
	private IModerationLogService _modService;
	private IForumService _forumService;
	private IEventPublisher _eventPublisher;
	private ISearchRepository _searchRepo;
	private IUserRepository _userRepo;
	private ISearchIndexQueueRepository _searchIndexQueueRepo;
	private ITenantService _tenantService;
	private INotificationAdapter _notificationAdapter;

	private TopicService GetTopicService()
	{
		_settingsManager = Substitute.For<ISettingsManager>();
		_topicRepo = Substitute.For<ITopicRepository>();
		_postRepo = Substitute.For<IPostRepository>();
		_modService = Substitute.For<IModerationLogService>();
		_forumService = Substitute.For<IForumService>();
		_eventPublisher = Substitute.For<IEventPublisher>();
		_searchRepo = Substitute.For<ISearchRepository>();
		_userRepo = Substitute.For<IUserRepository>();
		_searchIndexQueueRepo = Substitute.For<ISearchIndexQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		_notificationAdapter = Substitute.For<INotificationAdapter>();
		return new TopicService(_topicRepo, _postRepo, _settingsManager, _modService, _forumService, _eventPublisher, _searchRepo, _userRepo, _searchIndexQueueRepo, _tenantService, _notificationAdapter);
	}

	private static User GetUser()
	{
		return new User { UserID = 123, Name = "Name", Email = "Email", IsApproved = true, AuthorizationKey = Guid.NewGuid(), Roles = new List<string>()};
	}

	[Fact]
	public async Task GetTopicsFromRepo()
	{
		var forum = new Forum { ForumID = 1, TopicCount = 3 };
		var topicService = GetTopicService();
		var repoTopics = new List<Topic>();
		var settings = new Settings {TopicsPerPage = 20};
		_topicRepo.Get(1, true, 1, settings.TopicsPerPage).Returns(Task.FromResult(repoTopics));
		_settingsManager.Current.Returns(settings);
		var (topics, _) = await topicService.GetTopics(forum, true, 1);
		Assert.Same(repoTopics, topics);
	}

	[Fact]
	public async Task GetTopicsStartRowCalcd()
	{
		var forum = new Forum { ForumID = 1, TopicCount = 300 };
		var topicService = GetTopicService();
		var settings = new Settings { TopicsPerPage = 20 };
		_settingsManager.Current.Returns(settings);
		await topicService.GetTopics(forum, false, 3);
		await _topicRepo.Received().Get(Arg.Any<int>(), Arg.Any<bool>(), 41, Arg.Any<int>());
	}

	[Fact]
	public async Task GetTopicsIncludeDeletedCallsRepoCount()
	{
		var forum = new Forum { ForumID = 1 };
		var topicService = GetTopicService();
		_topicRepo.Get(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(new List<Topic>()));
		_topicRepo.GetTopicCount(1, true).Returns(Task.FromResult(350));
		_settingsManager.Current.Returns(new Settings());
		await topicService.GetTopics(forum, true, 3);
		await _topicRepo.Received().GetTopicCount(forum.ForumID, true);
	}

	[Fact]
	public async Task GetTopicsNotIncludeDeletedNotCallRepoCount()
	{
		var forum = new Forum { ForumID = 1 };
		var topicService = GetTopicService();
		_topicRepo.Get(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(new List<Topic>()));
		_settingsManager.Current.Returns(new Settings());
		await topicService.GetTopics(forum, false, 3);
		await _topicRepo.DidNotReceive().GetTopicCount(forum.ForumID, false);
	}

	[Fact]
	public async Task GetTopicsPagerContextIncludesPageIndexAndCalcdTotalPages()
	{
		var forum = new Forum {ForumID = 1, TopicCount = 301};
		var forum2 = new Forum {ForumID = 2, TopicCount = 300};
		var forum3 = new Forum {ForumID = 3, TopicCount = 299};
		var topicService = GetTopicService();
		var settings = new Settings { TopicsPerPage = 20 };
		_topicRepo.Get(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<int>(), settings.TopicsPerPage).Returns(Task.FromResult(new List<Topic>()));
		_settingsManager.Current.Returns(settings);
		var (_, pagerContext) = await topicService.GetTopics(forum, false, 3);
		Assert.Equal(3, pagerContext.PageIndex);
		Assert.Equal(16, pagerContext.PageCount);
		(_, pagerContext) = await topicService.GetTopics(forum2, false, 4);
		Assert.Equal(4, pagerContext.PageIndex);
		Assert.Equal(15, pagerContext.PageCount);
		(_, pagerContext) = await topicService.GetTopics(forum3, false, 5);
		Assert.Equal(5, pagerContext.PageIndex);
		Assert.Equal(15, pagerContext.PageCount);
		Assert.Equal(settings.TopicsPerPage, pagerContext.PageSize);
	}

	[Fact]
	public async Task CloseTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.CloseTopic(topic, user));
	}

	[Fact]
	public async Task CloseTopicClosesWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.CloseTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicClose, topic, null);
		await _topicRepo.Received(1).CloseTopic(topic.TopicID);
	}

	[Fact]
	public async Task OpenTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.OpenTopic(topic, user));
	}

	[Fact]
	public async Task OpenTopicOpensWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.OpenTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicOpen, topic, null);
		await _topicRepo.Received(1).OpenTopic(topic.TopicID);
	}

	[Fact]
	public async Task PinTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.PinTopic(topic, user));
	}

	[Fact]
	public async Task PinTopicPinsWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.PinTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicPin, topic, null);
		await _topicRepo.Received(1).PinTopic(topic.TopicID);
	}

	[Fact]
	public async Task UnpinTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UnpinTopic(topic, user));
	}

	[Fact]
	public async Task UnpinTopicUnpinsWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.UnpinTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicUnpin, topic, null);
		await _topicRepo.Received(1).UnpinTopic(topic.TopicID);
	}

	[Fact]
	public async Task DeleteTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.DeleteTopic(topic, user));
	}

	[Fact]
	public async Task DeleteTopicDeletesWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.DeleteTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicDelete, topic, null);
		await _topicRepo.Received(1).DeleteTopic(topic.TopicID);
	}

	[Fact]
	public async Task DeleteTopicUpdatesCounts()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		await topicService.DeleteTopic(topic, user);
		_forumService.Received(1).UpdateCounts(forum);
	}

	[Fact]
	public async Task DeleteTopicQueuesIndexRemoval()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		SearchIndexPayload payload = null;
		await _searchIndexQueueRepo.Enqueue(Arg.Do<SearchIndexPayload>(x => payload = x));
		_tenantService.GetTenant().Returns("t");

		await topicService.DeleteTopic(topic, user);

		Assert.Equal(topic.TopicID, payload.TopicID);
		Assert.Equal("t", payload.TenantID);
		Assert.True(payload.IsForRemoval);
	}

	[Fact]
	public async Task DeleteTopicUpdatesLast()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		await topicService.DeleteTopic(topic, user);
		await _forumService.Received(1).UpdateLast(forum);
	}

	[Fact]
	public async Task DeleteTopicUpdatesReplyCount()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_postRepo.GetReplyCount(topic.TopicID, false).Returns(Task.FromResult(42));
		await topicService.DeleteTopic(topic, user);
		await _topicRepo.Received(1).UpdateReplyCount(topic.TopicID, 42);
	}

	[Fact]
	public async Task DeleteTopicDeletesWithStarter()
	{
		var user = GetUser();
		var topic = new Topic { TopicID = 1, StartedByUserID = user.UserID };
		var topicService = GetTopicService();
		await topicService.DeleteTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicDelete, topic, null);
		await _topicRepo.Received(1).DeleteTopic(topic.TopicID);
	}

	[Fact]
	public async Task UndeleteTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UndeleteTopic(topic, user));
	}

	[Fact]
	public async Task UndeleteTopicUndeletesWithMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		await topicService.UndeleteTopic(topic, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicUndelete, topic, null);
		await _topicRepo.Received(1).UndeleteTopic(topic.TopicID);
	}

	[Fact]
	public async Task UndeleteTopicUpdatesCounts()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		await topicService.UndeleteTopic(topic, user);
		_forumService.Received(1).UpdateCounts(forum);
	}

	[Fact]
	public async Task UndeleteTopicUpdatesLast()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		await topicService.UndeleteTopic(topic, user);
		await _forumService.Received(1).UpdateLast(forum);
	}

	[Fact]
	public async Task UndeleteQueuesReindex()
	{
		var topic = new Topic { TopicID = 1, ForumID = 123 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		var forum = new Forum { ForumID = topic.ForumID };
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		SearchIndexPayload payload = null;
		await _searchIndexQueueRepo.Enqueue(Arg.Do<SearchIndexPayload>(x => payload = x));
		_tenantService.GetTenant().Returns("t");

		await topicService.UndeleteTopic(topic, user);

		Assert.Equal(topic.TopicID, payload.TopicID);
		Assert.Equal("t", payload.TenantID);
		Assert.False(payload.IsForRemoval);
	}

	[Fact]
	public async Task UndeleteTopicUpdatesReplyCount()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_postRepo.GetReplyCount(topic.TopicID, false).Returns(Task.FromResult(42));
		await topicService.UndeleteTopic(topic, user);
		await _topicRepo.Received(1).UpdateReplyCount(topic.TopicID, 42);
	}

	[Fact]
	public async Task UpdateTopicThrowsWithNonMod()
	{
		var topic = new Topic { TopicID = 1 };
		var user = GetUser();
		var topicService = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UpdateTitleAndForum(topic, new Forum { ForumID = 2 }, "blah", user));
	}

	[Fact]
	public async Task UpdateTopicUpdatesTitleWithMod()
	{
		var forum = new Forum { ForumID = 2 };
		var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = 2 }));
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicRenamed, topic, forum, Arg.Any<string>());
		await _topicRepo.Received(1).UpdateTitleAndForum(topic.TopicID, forum.ForumID, "new title", "new-title");
	}

	[Fact]
	public async Task UpdateTopicQueuesTopicForIndexingWithMod()
	{
		var forum = new Forum { ForumID = 2 };
		var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = 2 }));
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		_tenantService.GetTenant().Returns("");
		await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
		await _searchIndexQueueRepo.Received().Enqueue(Arg.Any<SearchIndexPayload>());
	}

	[Fact]
	public async Task UpdateTopicMovesTopicWithMod()
	{
		var forum = new Forum { ForumID = 2 };
		var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = 3 }));
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		await topicService.UpdateTitleAndForum(topic, forum, string.Empty, user);
		await _modService.Received(1).LogTopic(user, ModerationType.TopicMoved, topic, forum, Arg.Any<string>());
		await _topicRepo.Received(1).UpdateTitleAndForum(topic.TopicID, forum.ForumID, String.Empty, String.Empty);
	}

	[Fact]
	public async Task UpdateTopicWithNewTitleChangesUrlNameOnTopicParameter()
	{
		var forum = new Forum { ForumID = 2 };
		var topic = new Topic { TopicID = 1, ForumID = forum.ForumID, UrlName = "old" };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = 2 }));
		await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
		Assert.Equal("new-title", topic.UrlName);
	}

	[Fact]
	public async Task UpdateTopicMovesUpdatesCountAndLastOnOldForum()
	{
		var forum = new Forum { ForumID = 2 };
		var oldForum = new Forum { ForumID = 3 };
		var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = oldForum.ForumID }));
		_forumService.Get(oldForum.ForumID).Returns(Task.FromResult(oldForum));
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		await topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
		_forumService.Received(1).UpdateCounts(forum);
		await _forumService.Received(1).UpdateLast(forum);
	}

	[Fact]
	public async Task UpdateTopicMovesUpdatesCountAndLastOnNewForum()
	{
		var forum = new Forum { ForumID = 2 };
		var oldForum = new Forum { ForumID = 3 };
		var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
		var user = GetUser();
		user.Roles.Add(PermanentRoles.Moderator);
		var topicService = GetTopicService();
		_topicRepo.Get(topic.TopicID).Returns(Task.FromResult(new Topic { TopicID = 1, ForumID = oldForum.ForumID }));
		_forumService.Get(oldForum.ForumID).Returns(Task.FromResult(oldForum));
		_topicRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		await topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
		_forumService.Received(1).UpdateCounts(oldForum);
		await _forumService.Received(1).UpdateLast(oldForum);
	}

	[Fact]
	public async Task UpdateLastSetsFieldsFromLastPost()
	{
		var topic = new Topic { TopicID = 456 };
		var post = new Post { PostID = 123, TopicID = topic.TopicID, UserID = 789, Name = "Dude", PostTime = new DateTime(2000, 1, 3)};
		var service = GetTopicService();
		_postRepo.GetLastInTopic(post.TopicID).Returns(Task.FromResult(post));
		await service.UpdateLast(topic);
		await _topicRepo.Received().UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime);
	}

	[Fact]
	public async Task HardDeleteThrowsIfUserNotAdmin()
	{
		var user = new User { UserID = 123, Roles = new List<string>() };
		var topic = new Topic { TopicID = 45 };
		var service = GetTopicService();
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.HardDeleteTopic(topic, user));
	}

	[Fact]
	public async Task HardDeleteCallsModerationService()
	{
		var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
		var topic = new Topic { TopicID = 45 };
		var service = GetTopicService();
		await service.HardDeleteTopic(topic, user);
		await _modService.Received().LogTopic(user, ModerationType.TopicDeletePermanently, topic, null);
	}

	[Fact]
	public async Task HardDeleteCallsSearchIndexRepo()
	{
		var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
		var topic = new Topic { TopicID = 45 };
		var service = GetTopicService();
		SearchIndexPayload payload = null;
		await _searchIndexQueueRepo.Enqueue(Arg.Do<SearchIndexPayload>(x => payload = x));
		_tenantService.GetTenant().Returns("t");

		await service.HardDeleteTopic(topic, user);

		await _searchIndexQueueRepo.Received().Enqueue(Arg.Any<SearchIndexPayload>());
		Assert.Equal(topic.TopicID, payload.TopicID);
		Assert.Equal("t", payload.TenantID);
		Assert.True(payload.IsForRemoval);
	}

	[Fact]
	public async Task HardDeleteCallsTopiRepoToDeleteTopic()
	{
		var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
		var topic = new Topic { TopicID = 45 };
		var service = GetTopicService();
		await service.HardDeleteTopic(topic, user);
		await _topicRepo.Received().HardDeleteTopic(topic.TopicID);
	}

	[Fact]
	public async Task HardDeleteCallsForumServiceToUpdateLastAndCounts()
	{
		var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
		var topic = new Topic { TopicID = 45, ForumID = 67};
		var forum = new Forum { ForumID = topic.ForumID };
		var service = GetTopicService();
		_forumService.Get(topic.ForumID).Returns(Task.FromResult(forum));
		await service.HardDeleteTopic(topic, user);
		_forumService.Received().UpdateCounts(forum);
		await _forumService.Received().UpdateLast(forum);
	}

	[Fact]
	public async Task SetAnswerThrowsWhenUserNotTopicStarter()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456, StartedByUserID = 789 };
		await Assert.ThrowsAsync<SecurityException>(async () => await service.SetAnswer(user, topic, new Post { PostID = 789 }, "", ""));
	}

	[Fact]
	public async Task SetAnswerThrowsIfPostIDOfAnswerDoesntExist()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
		_postRepo.Get(Arg.Any<int>()).Returns((Post) null);
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, new Post { PostID = 789 }, "", ""));
	}

	[Fact]
	public async Task SetAnswerThrowsIfPostIsNotPartOfTopic()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
		var post = new Post { PostID = 789, TopicID = 111 };
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, post, "", ""));
	}

	[Fact]
	public async Task SetAnswerCallsTopicRepoWithUpdatedValue()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123, Name = "Dude" };
		var topic = new Topic { TopicID = 456, StartedByUserID = 123, Title = "the title" };
		var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = 777 };
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));

		await service.SetAnswer(user, topic, post, "", "");

		await _topicRepo.Received().UpdateAnswerPostID(topic.TopicID, post.PostID);
		await _notificationAdapter.Received().QuestionAnswer(user.Name, topic.Title, post.PostID, post.UserID);
	}

	[Fact]
	public async Task SetAnswerCallsEventPubWhenThereIsNoPreviousAnswerOnTheTopic()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var answerUser = new User { UserID = 777 };
		var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null};
		var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = answerUser.UserID};
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));
		_userRepo.GetUser(answerUser.UserID).Returns(Task.FromResult(answerUser));
		await service.SetAnswer(user, topic, post, "", "");
		await _eventPublisher.Received().ProcessEvent(Arg.Any<string>(), answerUser, EventDefinitionService.StaticEventIDs.QuestionAnswered, false);
	}

	[Fact]
	public async Task SetAnswerDoesNotCallEventPubWhenTheAnswerUserDoesNotExist()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null };
		var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = 777 };
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));
		_userRepo.GetUser(Arg.Any<int>()).Returns((User)null);
		await service.SetAnswer(user, topic, post, "", "");
		await _eventPublisher.DidNotReceive().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>());
	}

	[Fact]
	public async Task SetAnswerDoesNotCallEventPubWhenTheTopicAlreadyHasAnAnswer()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var answerUser = new User { UserID = 777 };
		var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = 666 };
		var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = answerUser.UserID };
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));
		_userRepo.GetUser(answerUser.UserID).Returns(Task.FromResult(answerUser));
		await service.SetAnswer(user, topic, post, "", "");
		await _eventPublisher.DidNotReceive().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>());
	}

	[Fact]
	public async Task SetAnswerDoesNotCallEventPubWhenTopicUserIDIsSameAsAnswerUserID()
	{
		var service = GetTopicService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null };
		var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = user.UserID };
		_postRepo.Get(post.PostID).Returns(Task.FromResult(post));
		_userRepo.GetUser(user.UserID).Returns(Task.FromResult(user));
		await service.SetAnswer(user, topic, post, "", "");
		await _eventPublisher.DidNotReceive().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>());
	}

	[Fact]
	public async Task CloseAgedTopicsDoesNothingWhenSettingIsOff()
	{
		var service = GetTopicService();
		_settingsManager.Current.IsClosingAgedTopics.Returns(false);

		await service.CloseAgedTopics();

		await _topicRepo.DidNotReceive().CloseTopicsOlderThan(Arg.Any<DateTime>());
	}

	[Fact]
	public async Task CloseAgedTopicsCallsRepoWhenSettingIsOn()
	{
		var service = GetTopicService();
		_settingsManager.Current.IsClosingAgedTopics.Returns(true);
		_topicRepo.CloseTopicsOlderThan(Arg.Any<DateTime>()).Returns(new List<int>());

		await service.CloseAgedTopics();

		await _topicRepo.Received().CloseTopicsOlderThan(Arg.Any<DateTime>());
	}

	[Fact]
	public async Task CloseAgedTopicsLogsTopicModeration()
	{
		var service = GetTopicService();
		_settingsManager.Current.IsClosingAgedTopics.Returns(true);
		_topicRepo.CloseTopicsOlderThan(Arg.Any<DateTime>()).Returns(new List<int>{1,2,3});

		await service.CloseAgedTopics();

		await _topicRepo.Received().CloseTopicsOlderThan(Arg.Any<DateTime>());
		await _modService.Received().LogTopic(ModerationType.TopicCloseAuto, 1);
		await _modService.Received().LogTopic(ModerationType.TopicCloseAuto, 2);
		await _modService.Received().LogTopic(ModerationType.TopicCloseAuto, 3);
	}
}