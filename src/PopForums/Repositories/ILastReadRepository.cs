using System;
using System.Collections.Generic;

namespace PopForums.Repositories
{
	public interface ILastReadRepository
	{
		void SetForumRead(int userID, int forumID, DateTime readTime);
		void DeleteTopicReadsInForum(int userID, int forumID);
		void SetAllForumsRead(int userID, DateTime readTime);
		void DeleteAllTopicReads(int userID);
		void SetTopicRead(int userID, int topicID, DateTime readTime);
		Dictionary<int, DateTime> GetLastReadTimesForForums(int userID);
		DateTime? GetLastReadTimesForForum(int userID, int forumID);
		Dictionary<int, DateTime> GetLastReadTimesForTopics(int userID, IEnumerable<int> topicIDs);
		DateTime? GetLastReadTimeForTopic(int userID, int topicID);
	}
}