using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ITopicRepository
	{
		Topic GetLastUpdatedTopic(int forumID);
		int GetTopicCount(int forumID, bool includeDeleted);
		int GetTopicCountByUser(int userID, bool includeDeleted, List<int> excludedForums);
		int GetTopicCount(bool includeDeleted, List<int> excludedForums);
		int GetPostCount(int forumID, bool includeDelete);
		Topic Get(int topicID);
		Topic Get(string urlName);
		List<Topic> Get(int forumID, bool includeDeleted, int startRow, int pageSize);
		List<Topic> GetTopicsByUser(int userID, bool includeDeleted, List<int> excludedForums, int startRow, int pageSize);
		List<Topic> Get(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize);
		List<Topic> Get(int forumID, bool includeDeleted, List<int> excludedForums);
		List<string> GetUrlNamesThatStartWith(string urlName);
		int Create(int forumID, string title, int replyCount, int viewCount, int startedByUserID, string startedByName, int lastPostUserID, string lastPostName, DateTime lastPostTime, bool isClosed, bool isPinned, bool isDeleted, string urlName);
		void IncrementReplyCount(int topicID);
		void IncrementViewCount(int topicID);
		void UpdateLastTimeAndUser(int topicID, int userID, string name, DateTime postTime);
		void CloseTopic(int topicID);
		void OpenTopic(int topicID);
		void PinTopic(int topicID);
		void UnpinTopic(int topicID);
		void DeleteTopic(int topicID);
		void UndeleteTopic(int topicID);
		void UpdateTitleAndForum(int topicID, int forumID, string newTitle, string newUrlName);
		void UpdateReplyCount(int topicID, int replyCount);
		DateTime? GetLastPostTime(int topicID);
		void HardDeleteTopic(int topicID);
		void UpdateAnswerPostID(int topicID, int? postID);
		List<Topic> Get(IEnumerable<int> topicIDs);
	}
}
