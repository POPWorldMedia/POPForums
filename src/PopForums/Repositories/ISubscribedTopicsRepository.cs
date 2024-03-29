﻿namespace PopForums.Repositories;

public interface ISubscribedTopicsRepository
{
	Task<List<Topic>> GetSubscribedTopics(int userID, int startRow, int pageSize);
	Task<int> GetSubscribedTopicCount(int userID);
	Task<bool> IsTopicSubscribed(int userID, int topicID);
	Task AddSubscribedTopic(int userID, int topicID);
	Task RemoveSubscribedTopic(int userID, int topicID);
	Task<List<int>> GetSubscribedUserIDs(int topicID);
}