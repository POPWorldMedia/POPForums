using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IPostService
	{
		List<Post> GetPosts(Topic topic, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		List<Post> GetPosts(Topic topic, int lastLoadedPostID, bool includeDeleted, out PagerContext pagerContext);
		List<Post> GetPosts(Topic topic, bool includeDeleted);
		Post Get(int postID);
		List<Post> GetPostWithReplies(int id, bool includeDeleted);
		Post GetFirstInTopic(Topic topic);
		bool IsNewPostDupeOrInTimeLimit(NewPost newPost, User user);
		int GetTopicPageForPost(Post post, bool includeDeleted, out Topic topic);
		int GetPostCount(User user);
		PostEdit GetPostForEdit(Post post, User user, bool isMobile);
		void EditPost(Post post, PostEdit postEdit, User editingUser);
		void Delete(Post post, User user);
		void Undelete(Post post, User user);
		string GetPostForQuote(Post post, User user, bool forcePlainText);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
		int GetLastPostID(int topicID);
		void VotePost(Post post, User user, string userUrl, string topicUrl, string topicTitle);
		VotePostContainer GetVoters(Post post);
		int GetVoteCount(Post post);
		List<int> GetVotedPostIDs(User user, List<Post> posts);
	}
}