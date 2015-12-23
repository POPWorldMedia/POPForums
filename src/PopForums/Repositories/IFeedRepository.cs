using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFeedRepository
	{
		List<FeedEvent> GetFeed(int userID, int itemCount);
		void PublishEvent(int userID, string message, int points, DateTime timeStamp);
		DateTime GetOldestTime(int userID, int takeCount);
		void DeleteOlderThan(int userID, DateTime timeCutOff);
		List<FeedEvent> GetFeed(int itemCount);
	}
}