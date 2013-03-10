using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ISubscribedTopicsService
	{
		void AddSubscribedTopic(User user, Topic topic);
		void RemoveSubscribedTopic(User user, Topic topic);
		void TryRemoveSubscribedTopic(User user, Topic topic);
		void MarkSubscribedTopicViewed(User user, Topic topic);
		void NotifySubscribers(Topic topic, User postingUser, string topicLink, Func<User, string> unsubscribeLinkGenerator);
		List<Topic> GetTopics(User user, int pageIndex, out PagerContext pagerContext);
		bool IsTopicSubscribed(User user, Topic topic);
	}
}