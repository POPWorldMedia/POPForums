namespace PopForums.Test.Services;

public class TopicViewLogServiceTests
{
	private TopicViewLogService GetService()
	{
		_config = Substitute.For<IConfig>();
		_topicViewLogRepo = Substitute.For<ITopicViewLogRepository>();
		return new TopicViewLogService(_config, _topicViewLogRepo);
	}

	private IConfig _config;
	private ITopicViewLogRepository _topicViewLogRepo;

	[Fact]
	public async Task LogViewDoesNotCallRepoWhenConfigIsFalse()
	{
		var service = GetService();
		_config.LogTopicViews.Returns(false);

		await service.LogView(123, 456);

		await _topicViewLogRepo.DidNotReceive().Log(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<DateTime>());
	}

	[Fact]
	public async Task LogViewDoesCallsRepoWhenConfigIsTrue()
	{
		var service = GetService();
		_config.LogTopicViews.Returns(true);

		await service.LogView(123, 456);

		await _topicViewLogRepo.Received().Log(123, 456, Arg.Any<DateTime>());
	}
}