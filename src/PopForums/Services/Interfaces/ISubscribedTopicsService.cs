namespace PopForums.Services.Interfaces;

public interface ISubscribedTopicsService
{
    Task AddSubscribedTopic(int userID, int topicID);

    Task RemoveSubscribedTopic(User user, Topic topic);

    Task TryRemoveSubscribedTopic(User user, Topic topic);

    Task NotifySubscribers(Topic topic, User postingUser, string tenantID);

    Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex);

    Task<bool> IsTopicSubscribed(int userID, int topicID);

    Task<List<int>> GetSubscribedUserIDs(int topicID);
}
