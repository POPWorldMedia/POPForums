using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ITopicRepository
	{
		Task<Topic> GetLastUpdatedTopic(int forumID);
		Task<int> GetTopicCount(int forumID, bool includeDeleted);
		Task<int> GetTopicCountByUser(int userID, bool includeDeleted, List<int> excludedForums);
		Task<int> GetTopicCount(bool includeDeleted, List<int> excludedForums);
		Task<int> GetPostCount(int forumID, bool includeDelete);
		Task<Topic> Get(int topicID);
		Task<Topic> Get(string urlName);
		Task<List<Topic>> Get(int forumID, bool includeDeleted, int startRow, int pageSize);
		Task<List<Topic>> GetTopicsByUser(int userID, bool includeDeleted, List<int> excludedForums, int startRow, int pageSize);
		Task<List<Topic>> Get(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize);
		Task<List<Topic>> Get(int forumID, bool includeDeleted, List<int> excludedForums);
		Task<List<string>> GetUrlNamesThatStartWith(string urlName);
		Task<int> Create(int forumID, string title, int replyCount, int viewCount, int startedByUserID, string startedByName, int lastPostUserID, string lastPostName, DateTime lastPostTime, bool isClosed, bool isPinned, bool isDeleted, string urlName);
		Task IncrementReplyCount(int topicID);
		Task IncrementViewCount(int topicID);
		Task UpdateLastTimeAndUser(int topicID, int userID, string name, DateTime postTime);
		Task CloseTopic(int topicID);
		Task OpenTopic(int topicID);
		Task PinTopic(int topicID);
		Task UnpinTopic(int topicID);
		Task DeleteTopic(int topicID);
		Task UndeleteTopic(int topicID);
		Task UpdateTitleAndForum(int topicID, int forumID, string newTitle, string newUrlName);
		Task UpdateReplyCount(int topicID, int replyCount);
		Task<DateTime?> GetLastPostTime(int topicID);
		Task HardDeleteTopic(int topicID);
		Task UpdateAnswerPostID(int topicID, int? postID);
		Task<List<Topic>> Get(IEnumerable<int> topicIDs);
		Task<IEnumerable<int>> CloseTopicsOlderThan(DateTime cutoffDate);
		Task<List<Tuple<string, DateTime>>> GetUrlNames(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize);
	}
}
