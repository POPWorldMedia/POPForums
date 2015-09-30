using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISubscribedTopicsRepository
	{
		List<Topic> GetSubscribedTopics(int userID, int startRow, int pageSize);
		int GetSubscribedTopicCount(int userID);
		List<User> GetSubscribedUsersThatHaveViewed(int topicID);
		bool IsTopicSubscribed(int userID, int topicID);
		void AddSubscribedTopic(int userID, int topicID);
		void RemoveSubscribedTopic(int userID, int topicID);
		void MarkSubscribedTopicViewed(int userID, int topicID);
		void MarkSubscribedTopicUnviewed(int topicID);
	}
}
