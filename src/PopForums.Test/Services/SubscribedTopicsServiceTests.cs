namespace PopForums.Test.Services;

public class SubscribedTopicsServiceTests
{
	private ISubscribedTopicsRepository _mockSubRepo;
	private ISettingsManager _mockSettingsManager;
	private INotificationAdapter _mockNotificationAdapter;
	private ISubscribeNotificationRepository _subNotificationRepo;

	private SubscribedTopicsService GetService()
	{
		_mockSubRepo = Substitute.For<ISubscribedTopicsRepository>();
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		_mockNotificationAdapter = Substitute.For<INotificationAdapter>();
		_subNotificationRepo = Substitute.For<ISubscribeNotificationRepository>();
		return new SubscribedTopicsService(_mockSubRepo, _mockSettingsManager, _mockNotificationAdapter, _subNotificationRepo);
	}

	[Fact]
	public async Task AddSubTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		_mockSubRepo.IsTopicSubscribed(user.UserID, topic.TopicID).Returns(Task.FromResult(false));

		await service.AddSubscribedTopic(user.UserID, topic.TopicID);

		await _mockSubRepo.Received().AddSubscribedTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task DoNotAddSubTopicIfAlreadySub()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		_mockSubRepo.IsTopicSubscribed(user.UserID, topic.TopicID).Returns(Task.FromResult(true));

		await service.AddSubscribedTopic(user.UserID, topic.TopicID);

		await _mockSubRepo.DidNotReceive().AddSubscribedTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task RemoveSubTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		await service.RemoveSubscribedTopic(user, topic);
		await _mockSubRepo.Received().RemoveSubscribedTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task TryRemoveSubTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		await service.TryRemoveSubscribedTopic(user, topic);
		await _mockSubRepo.Received().RemoveSubscribedTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task TryRemoveSubTopicNullTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		await service.TryRemoveSubscribedTopic(user, null);
		await _mockSubRepo.DidNotReceive().RemoveSubscribedTopic(Arg.Any<int>(), Arg.Any<int>());
	}

	[Fact]
	public async Task TryRemoveSubTopicNullUser()
	{
		var service = GetService();
		var topic = new Topic { TopicID = 456 };
		await service.TryRemoveSubscribedTopic(null, topic);
		await _mockSubRepo.DidNotReceive().RemoveSubscribedTopic(Arg.Any<int>(), Arg.Any<int>());
	}
		
	[Fact]
	public async Task GetTopicsFromRepo()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var settings = new Settings { TopicsPerPage = 20 };
		_mockSettingsManager.Current.Returns(settings);
		var list = new List<Topic>();
		_mockSubRepo.GetSubscribedTopics(user.UserID, 1, 20).Returns(Task.FromResult(list));
		var (result, _) = await service.GetTopics(user, 1);
		Assert.Same(list, result);
	}

	[Fact]
	public async Task GetTopicsStartRowCalcd()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var settings = new Settings { TopicsPerPage = 20 };
		_mockSettingsManager.Current.Returns(settings);
		var (_, pagerContext) = await service.GetTopics(user, 3);
		await _mockSubRepo.Received().GetSubscribedTopics(user.UserID, 41, 20);
		Assert.Equal(20, pagerContext.PageSize);
	}
}