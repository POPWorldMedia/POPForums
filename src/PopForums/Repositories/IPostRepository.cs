using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IPostRepository
	{
		Task<int> Create(int topicID, int parentPostID, string IP, bool isFirstInTopic, bool showSig, int userID, string name, string title, string fullText, DateTime postTime, bool isEdited, string lastEditName, DateTime? lastEditTime, bool isDeleted, int votes);
		Task<bool> Update(Post post);
		Task<List<Post>> Get(int topicID, bool includeDeleted, int startRow, int pageSize);
		Task<List<Post>> Get(int topicID, bool includeDeleted);
		Task<int> GetReplyCount(int topicID, bool includeDeleted);
		Task<Post> Get(int postID);
		Task<Dictionary<int, DateTime>> GetPostIDsWithTimes(int topicID, bool includeDeleted);
		int GetPostCount(int userID);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
		int GetLastPostID(int topicID);
		int GetVoteCount(int postID);
		int CalculateVoteCount(int postID);
		void SetVoteCount(int postID, int votes);
		void VotePost(int postID, int userID);
		Dictionary<int, string> GetVotes(int postID);
		List<int> GetVotedPostIDs(int userID, List<int> postIDs);
		Task<Post> GetLastInTopic(int topicID);
	}
}
