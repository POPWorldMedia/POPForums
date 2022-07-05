namespace PopForums.Feeds;

public interface IFeedService
{
	Task PublishToFeed(User user, string message, int points, DateTime timeStamp);
	Task<List<FeedEvent>> GetFeed(User user);
	void PublishToActivityFeed(string message);
}

public class FeedService : IFeedService
{

	public FeedService(IFeedRepository feedRepository, IBroker broker)
	{
		_feedRepository = feedRepository;
		_broker = broker;
	}

	private readonly IFeedRepository _feedRepository;
	private readonly IBroker _broker;
		
	public const int MaxFeedCount = 50;

	public async Task PublishToFeed(User user, string message, int points, DateTime timeStamp)
	{
		if (user == null)
			return;
		await _feedRepository.PublishEvent(user.UserID, message, points, timeStamp);
		var cutOff = await _feedRepository.GetOldestTime(user.UserID, MaxFeedCount);
		await _feedRepository.DeleteOlderThan(user.UserID, cutOff);
	}

	public async Task<List<FeedEvent>> GetFeed(User user)
	{
		return await _feedRepository.GetFeed(user.UserID, MaxFeedCount);
	}

	public void PublishToActivityFeed(string message)
	{
		_broker.NotifyFeed(message);
	}
}