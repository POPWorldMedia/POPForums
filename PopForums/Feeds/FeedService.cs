using System;
using System.Collections.Generic;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Feeds
{
	public class FeedService : IFeedService
	{
		public FeedService(IFeedRepository feedRepository, IBroker broker)
		{
			_feedRepository = feedRepository;
			_broker = broker;
		}

		private readonly IFeedRepository _feedRepository;
		private readonly IBroker _broker;
		
		public const int MaxFeedCount = 50;

		public void PublishToFeed(User user, string message, int points, DateTime timeStamp)
		{
			if (user == null)
				return;
			_feedRepository.PublishEvent(user.UserID, message, points, timeStamp);
			var cutOff = _feedRepository.GetOldestTime(user.UserID, MaxFeedCount);
			_feedRepository.DeleteOlderThan(user.UserID, cutOff);
		}

		public List<FeedEvent> GetFeed(User user)
		{
			return _feedRepository.GetFeed(user.UserID, MaxFeedCount);
		}

		public List<FeedEvent> GetFeed()
		{
			return _feedRepository.GetFeed(MaxFeedCount);
		}

		public void PublishToActivityFeed(string message)
		{
			_broker.NotifyFeed(message);
		}
	}
}