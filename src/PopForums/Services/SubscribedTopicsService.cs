using PopForums.Models;

namespace PopForums.Services;

public interface ISubscribedTopicsService
{
	Task AddSubscribedTopic(int userID, int topicID);
	Task RemoveSubscribedTopic(User user, Topic topic);
	Task TryRemoveSubscribedTopic(User user, Topic topic);
	Task MarkSubscribedTopicViewed(User user, Topic topic);
	Task NotifySubscribers(Topic topic, User postingUser, string topicLink, Func<User, Topic, string> unsubscribeLinkGenerator);
	Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex);
	Task<bool> IsTopicSubscribed(int userID, int topicID);
}

public class SubscribedTopicsService : ISubscribedTopicsService
{
	public SubscribedTopicsService(ISubscribedTopicsRepository subscribedTopicsRepository, ISubscribedTopicEmailComposer subscribedTopicEmailComposer, ISettingsManager settingsManager, INotificationAdapter notificationAdapter)
	{
		_subscribedTopicsRepository = subscribedTopicsRepository;
		_subscribedTopicEmailComposer = subscribedTopicEmailComposer;
		_settingsManager = settingsManager;
		_notificationAdapter = notificationAdapter;
	}

	private readonly ISubscribedTopicsRepository _subscribedTopicsRepository;
	private readonly ISubscribedTopicEmailComposer _subscribedTopicEmailComposer;
	private readonly ISettingsManager _settingsManager;
	private readonly INotificationAdapter _notificationAdapter;

	public async Task AddSubscribedTopic(int userID, int topicID)
	{
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

	public async Task MarkSubscribedTopicViewed(User user, Topic topic)
	{
		if (user == null || topic == null)
			return;
		await _subscribedTopicsRepository.MarkSubscribedTopicViewed(user.UserID, topic.TopicID);
	}

#pragma warning disable 1998
	public async Task NotifySubscribers(Topic topic, User postingUser, string topicLink, Func<User, Topic, string> unsubscribeLinkGenerator)
	{
		new Thread(async () => {
			// old emails
			var users = await _subscribedTopicsRepository.GetSubscribedUsersThatHaveViewed(topic.TopicID);
			foreach (var user in users)
			{
				if (user.UserID != postingUser.UserID)
				{
					var unsubScribeLink = unsubscribeLinkGenerator(user, topic);
					await _subscribedTopicEmailComposer.ComposeAndQueue(topic, user, topicLink, unsubScribeLink);
				}
			}
			// new notifications
			var userIDs = await _subscribedTopicsRepository.GetSubscribedUserIDs(topic.TopicID);
			foreach (var userID in userIDs)
				await _notificationAdapter.Reply(postingUser.Name, topic.Title, topic.TopicID, userID);
			await _subscribedTopicsRepository.MarkSubscribedTopicUnviewed(topic.TopicID);
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