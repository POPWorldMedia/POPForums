namespace PopForums.Test.Services;

public class FavoriteTopicServiceTests
{
	private IFavoriteTopicsRepository _mockFaveRepo;
	private ISettingsManager _mockSettingsManager;

	private FavoriteTopicService GetService()
	{
		_mockFaveRepo = Substitute.For<IFavoriteTopicsRepository>();
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		return new FavoriteTopicService(_mockSettingsManager, _mockFaveRepo);
	}

	[Fact]
	public async Task GetTopicsFromRepo()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var settings = new Settings { TopicsPerPage = 20 };
		_mockSettingsManager.Current.Returns(settings);
		var list = new List<Topic>();
		_mockFaveRepo.GetFavoriteTopics(user.UserID, 1, 20).Returns(Task.FromResult(list));
		var result = await service.GetTopics(user, 1);
		Assert.Same(list, result.Item1);
	}

	[Fact]
	public async Task AddFaveTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		await service.AddFavoriteTopic(user, topic);
		await _mockFaveRepo.Received().AddFavoriteTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task RemoveFaveTopic()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var topic = new Topic { TopicID = 456 };
		await service.RemoveFavoriteTopic(user, topic);
		await _mockFaveRepo.Received().RemoveFavoriteTopic(user.UserID, topic.TopicID);
	}

	[Fact]
	public async Task GetTopicsStartRowCalcd()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var settings = new Settings { TopicsPerPage = 20 };
		_mockSettingsManager.Current.Returns(settings);
		var result = await service.GetTopics(user, 3);
		await _mockFaveRepo.Received().GetFavoriteTopics(user.UserID, 41, 20);
		result.Item2.PageSize = settings.TopicsPerPage;
	}
}