using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Repositories
{
	public interface ILastReadRepository
	{
		Task SetForumRead(int userID, int forumID, DateTime readTime);
		Task DeleteTopicReadsInForum(int userID, int forumID);
		Task SetAllForumsRead(int userID, DateTime readTime);
		Task DeleteAllTopicReads(int userID);
		Task SetTopicRead(int userID, int topicID, DateTime readTime);
		Task<Dictionary<int, DateTime>> GetLastReadTimesForForums(int userID);
		Task<DateTime?> GetLastReadTimesForForum(int userID, int forumID);
		Task<Dictionary<int, DateTime>> GetLastReadTimesForTopics(int userID, IEnumerable<int> topicIDs);
		Task<DateTime?> GetLastReadTimeForTopic(int userID, int topicID);
	}
}