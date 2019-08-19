using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFeedRepository
	{
		Task<List<FeedEvent>> GetFeed(int userID, int itemCount);
		Task PublishEvent(int userID, string message, int points, DateTime timeStamp);
		Task<DateTime> GetOldestTime(int userID, int takeCount);
		Task DeleteOlderThan(int userID, DateTime timeCutOff);
		Task<List<FeedEvent>> GetFeed(int itemCount);
	}
}