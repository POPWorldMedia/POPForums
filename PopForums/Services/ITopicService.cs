using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ITopicService
	{
		List<Topic> GetTopics(Forum forum, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		Post PostReply(Topic topic, User user, int parentPostID, string ip, bool isFirstInTopic, NewPost newPost, DateTime postTime, string topicLink, Func<User, string> unsubscribeLinkGenerator, string userUrl, Func<Post, string> postLinkGenerator);
		Topic Get(string urlName);
		Topic Get(int topicID);
		void CloseTopic(Topic topic, User user);
		void OpenTopic(Topic topic, User user);
		void PinTopic(Topic topic, User user);
		void UnpinTopic(Topic topic, User user);
		void DeleteTopic(Topic topic, User user);
		void UndeleteTopic(Topic topic, User user);
		void UpdateTitleAndForum(Topic topic, Forum forum, string newTitle, User user);
		List<Topic> GetTopics(User viewingUser, User postUser, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		void RecalculateReplyCount(Topic topic);
		Dictionary<int, int> GetFirstPostIDsFromTopics(List<Topic> topics);
		DateTime? TopicLastPostTime(int topicID);
		List<Topic> GetTopics(User viewingUser, Forum forum, bool includeDeleted);
		void UpdateLast(Topic topic);
		int TopicLastPostID(int topicID);
	}
}