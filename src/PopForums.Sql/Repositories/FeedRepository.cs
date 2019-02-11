using System;
using System.Collections.Generic;
using System.Linq;
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

		public List<FeedEvent> GetFeed(int userID, int itemCount)
		{
			var list = new List<FeedEvent>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<FeedEvent>($"SELECT TOP {itemCount} UserID, Message, Points, TimeStamp FROM pf_Feed WHERE UserID = @UserID ORDER BY TimeStamp DESC", new { UserID = userID} ).ToList());
			return list;
		}

		public List<FeedEvent> GetFeed(int itemCount)
		{
			var list = new List<FeedEvent>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<FeedEvent>($"SELECT TOP {itemCount} UserID, Message, Points, TimeStamp FROM pf_Feed ORDER BY TimeStamp DESC").ToList());
			return list;
		}

		public void PublishEvent(int userID, string message, int points, DateTime timeStamp)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_Feed (UserID, Message, Points, TimeStamp) VALUES (@UserID, @Message, @Points, @TimeStamp)", new { UserID = userID, Message = message, Points = points, TimeStamp = timeStamp }));
		}

		public DateTime GetOldestTime(int userID, int takeCount)
		{
			var feed = GetFeed(userID, takeCount);
			if (feed.Count == 0)
				return new DateTime(1990, 1, 1);
			var last = feed.Last();
			return last.TimeStamp;
		}

		public void DeleteOlderThan(int userID, DateTime timeCutOff)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_Feed WHERE UserID = @UserID AND TimeStamp < @TimeStamp", new { UserID = userID, TimeStamp = timeCutOff }));
		}
	}
}