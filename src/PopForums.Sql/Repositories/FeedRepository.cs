using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class FeedRepository : IFeedRepository
	{
		public FeedRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task<List<FeedEvent>> GetFeed(int userID, int itemCount)
		{
			Task<IEnumerable<FeedEvent>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<FeedEvent>($"SELECT TOP {itemCount} UserID, Message, Points, TimeStamp FROM pf_Feed WHERE UserID = @UserID ORDER BY TimeStamp DESC", new { UserID = userID} ));
			return result.Result.ToList();
		}

		public async Task<List<FeedEvent>> GetFeed(int itemCount)
		{
			Task<IEnumerable<FeedEvent>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<FeedEvent>($"SELECT TOP {itemCount} UserID, Message, Points, TimeStamp FROM pf_Feed ORDER BY TimeStamp DESC"));
			return result.Result.ToList();
		}

		public async Task PublishEvent(int userID, string message, int points, DateTime timeStamp)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_Feed (UserID, Message, Points, TimeStamp) VALUES (@UserID, @Message, @Points, @TimeStamp)", new { UserID = userID, Message = message, Points = points, TimeStamp = timeStamp }));
		}

		public async Task<DateTime> GetOldestTime(int userID, int takeCount)
		{
			var feed = await GetFeed(userID, takeCount);
			if (feed.Count == 0)
				return new DateTime(1990, 1, 1);
			var last = feed.Last();
			return last.TimeStamp;
		}

		public async Task DeleteOlderThan(int userID, DateTime timeCutOff)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_Feed WHERE UserID = @UserID AND TimeStamp < @TimeStamp", new { UserID = userID, TimeStamp = timeCutOff }));
		}
	}
}