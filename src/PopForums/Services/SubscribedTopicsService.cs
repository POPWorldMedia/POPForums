namespace PopForums.Services;

public interface ISubscribedTopicsService
{
	Task AddSubscribedTopic(int userID, int topicID);
	Task RemoveSubscribedTopic(User user, Topic topic);
	Task TryRemoveSubscribedTopic(User user, Topic topic);
	Task NotifySubscribers(Topic topic, User postingUser);
	Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex);
	Task<bool> IsTopicSubscribed(int userID, int topicID);
}

public class SubscribedTopicsService : ISubscribedTopicsService
{
	public SubscribedTopicsService(ISubscribedTopicsRepository subscribedTopicsRepository, ISettingsManager settingsManager, INotificationAdapter notificationAdapter)
	{
		_subscribedTopicsRepository = subscribedTopicsRepository;
		_settingsManager = settingsManager;
		_notificationAdapter = notificationAdapter;
	}

	private readonly ISubscribedTopicsRepository _subscribedTopicsRepository;
	private readonly ISettingsManager _settingsManager;
	private readonly INotificationAdapter _notificationAdapter;

	public async Task AddSubscribedTopic(int userID, int topicID)
	{
		var isSubscribed = await _subscribedTopicsRepository.IsTopicSubscribed(userID, topicID);
		if (!isSubscribed)
			await _subscribedTopicsRepository.AddSubscribedTopic(userID, topicID);
	}

	public async Task RemoveSubscribedTopic(User user, Topic topic)
	{
		await _subscribedTopicsRepository.RemoveSubscribedTopic(user.UserID, topic.TopicID);
	}

	public async Task TryRemoveSubscribedTopic(User user, Topic topic)
	{
		if (user != null && topic != null)
			await RemoveSubscribedTopic(user, topic);
	}

	// TODO: there has to be a better way than this
#pragma warning disable 1998
	public async Task NotifySubscribers(Topic topic, User postingUser)
	{
		new Thread(async () => {
			// new notifications
			var userIDs = await _subscribedTopicsRepository.GetSubscribedUserIDs(topic.TopicID);
			var filteredUserIDs = userIDs.Where(x => x != postingUser.UserID);
			foreach (var userID in filteredUserIDs)
				await _notificationAdapter.Reply(postingUser.Name, topic.Title, topic.TopicID, userID);
		}).Start();
	}
#pragma warning restore 1998

	public async Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex)
	{
		var pageSize = _settingsManager.Current.TopicsPerPage;
		var startRow = ((pageIndex - 1) * pageSize) + 1;
		var topics = await _subscribedTopicsRepository.GetSubscribedTopics(user.UserID, startRow, pageSize);
		var topicCount = await _subscribedTopicsRepository.GetSubscribedTopicCount(user.UserID);
		var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
		var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
		return Tuple.Create(topics, pagerContext);
	}

	public async Task<bool> IsTopicSubscribed(int userID, int topicID)
	{
		return await _subscribedTopicsRepository.IsTopicSubscribed(userID, topicID);
	}
}