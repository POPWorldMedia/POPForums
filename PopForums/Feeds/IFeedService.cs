using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Feeds
{
	public interface IFeedService
	{
		void PublishToFeed(User user, string message, int points, DateTime timeStamp);
		List<FeedEvent> GetFeed(User user);
		List<FeedEvent> GetFeed();
		void PublishToActivityFeed(string message);
	}
}
