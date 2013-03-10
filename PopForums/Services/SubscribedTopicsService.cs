using System;
using System.Collections.Generic;
using System.Threading;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class SubscribedTopicsService : ISubscribedTopicsService
	{
		public SubscribedTopicsService(ISubscribedTopicsRepository subscribedTopicsRepository, ISubscribedTopicEmailComposer subscribedTopicEmailComposer, ISettingsManager settingsManager)
		{
			_subscribedTopicsRepository = subscribedTopicsRepository;
			_subscribedTopicEmailComposer = subscribedTopicEmailComposer;
			_settingsManager = settingsManager;
		}

		private readonly ISubscribedTopicsRepository _subscribedTopicsRepository;
		private readonly ISubscribedTopicEmailComposer _subscribedTopicEmailComposer;
		private readonly ISettingsManager _settingsManager;

		public void AddSubscribedTopic(User user, Topic topic)
		{
			_subscribedTopicsRepository.AddSubscribedTopic(user.UserID, topic.TopicID);
		}

		public void RemoveSubscribedTopic(User user, Topic topic)
		{
			_subscribedTopicsRepository.RemoveSubscribedTopic(user.UserID, topic.TopicID);
		}

		public void TryRemoveSubscribedTopic(User user, Topic topic)
		{
			if (user != null && topic != null)
				RemoveSubscribedTopic(user, topic);
		}

		public void MarkSubscribedTopicViewed(User user, Topic topic)
		{
			if (user == null || topic == null)
				return;
			_subscribedTopicsRepository.MarkSubscribedTopicViewed(user.UserID, topic.TopicID);
		}

		public void NotifySubscribers(Topic topic, User postingUser, string topicLink, Func<User, string> unsubscribeLinkGenerator)
		{
			new Thread(() => {
				var users = _subscribedTopicsRepository.GetSubscribedUsersThatHaveViewed(topic.TopicID);
				foreach (var user in users)
				{
					if (user.UserID != postingUser.UserID)
					{
						var unsubScribeLink = unsubscribeLinkGenerator(user);
						_subscribedTopicEmailComposer.ComposeAndQueue(topic, user, topicLink, unsubScribeLink);
					}
				}
				_subscribedTopicsRepository.MarkSubscribedTopicUnviewed(topic.TopicID);
			}).Start();
		}

		public List<Topic> GetTopics(User user, int pageIndex, out PagerContext pagerContext)
		{
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = _subscribedTopicsRepository.GetSubscribedTopics(user.UserID, startRow, pageSize);
			var topicCount = _subscribedTopicsRepository.GetSubscribedTopicCount(user.UserID);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public bool IsTopicSubscribed(User user, Topic topic)
		{
			return _subscribedTopicsRepository.IsTopicSubscribed(user.UserID, topic.TopicID);
		}
	}
}
