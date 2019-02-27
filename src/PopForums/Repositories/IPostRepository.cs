using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IPostRepository
	{
		int Create(int topicID, int parentPostID, string IP, bool isFirstInTopic, bool showSig, int userID, string name, string title, string fullText, DateTime postTime, bool isEdited, string lastEditName, DateTime? lastEditTime, bool isDeleted, int votes);
		bool Update(Post post);
		List<Post> Get(int topicID, bool includeDeleted, int startRow, int pageSize);
		List<Post> Get(int topicID, bool includeDeleted);
		List<Post> GetPostWithReplies(int postID, bool includeDeleted);
		Post GetFirstInTopic(int topicID);
		int GetReplyCount(int topicID, bool includeDeleted);
		Post Get(int postID);
		Dictionary<int, DateTime> GetPostIDsWithTimes(int topicID, bool includeDeleted);
		int GetPostCount(int userID);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
		Dictionary<int, int> GetFirstPostIDsFromTopicIDs(List<int> topicIDs);
		int GetLastPostID(int topicID);
		int GetVoteCount(int postID);
		int CalculateVoteCount(int postID);
		void SetVoteCount(int postID, int votes);
		void VotePost(int postID, int userID);
		Dictionary<int, string> GetVotes(int postID);
		List<int> GetVotedPostIDs(int userID, List<int> postIDs);
		Post GetLastInTopic(int topicID);
	}
}
