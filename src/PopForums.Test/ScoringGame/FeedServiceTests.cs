namespace PopForums.Test.ScoringGame;

public class FeedServiceTests
{
	private FeedService GetService()
	{
		_feedRepo = Substitute.For<IFeedRepository>();
		_broker = Substitute.For<IBroker>();
		return new FeedService(_feedRepo, _broker);
	}

	private IFeedRepository _feedRepo;
	private IBroker _broker;

	[Fact]
	public async Task PublishSavesToRepo()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		const string msg = "oiehgfoih";
		const int points = 5352;
		var timeStamp = new DateTime(2000, 1, 1);
		await service.PublishToFeed(user, msg, points, timeStamp);
		await _feedRepo.Received().PublishEvent(user.UserID, msg, points, timeStamp);
	}

	[Fact]
	public async Task PublishDeletesOlderThan50()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var timeStamp = new DateTime(2000, 1, 1);
		var cutOff = new DateTime(1999, 2, 2);
		const int points = 5352;
		_feedRepo.GetOldestTime(user.UserID, 50).Returns(Task.FromResult(cutOff));
		await service.PublishToFeed(user, "whatevs", points, timeStamp);
		await _feedRepo.Received().DeleteOlderThan(user.UserID, cutOff);
	}

	[Fact]
	public async Task PublishDoesNothingIfUserIsNull()
	{
		var service = GetService();
		await service.PublishToFeed(null, String.Empty, 423, new DateTime());
		await _feedRepo.DidNotReceive().PublishEvent(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateTime>());
	}

	[Fact]
	public async Task GetFeedGets50ItemsMaxFromRepo()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var list = new List<FeedEvent>();
		_feedRepo.GetFeed(user.UserID, 50).Returns(Task.FromResult(list));
		var result = await service.GetFeed(user);
		Assert.Same(result, list);
	}
}