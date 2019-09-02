using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISubscribedTopicsRepository
	{
		Task<List<Topic>> GetSubscribedTopics(int userID, int startRow, int pageSize);
		Task<int> GetSubscribedTopicCount(int userID);
		Task<List<User>> GetSubscribedUsersThatHaveViewed(int topicID);
		Task<bool> IsTopicSubscribed(int userID, int topicID);
		Task AddSubscribedTopic(int userID, int topicID);
		Task RemoveSubscribedTopic(int userID, int topicID);
		Task MarkSubscribedTopicViewed(int userID, int topicID);
		Task MarkSubscribedTopicUnviewed(int topicID);
	}
}
