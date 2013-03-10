using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
				connection.Command(String.Format("SELECT TOP {0} UserID, Message, Points, TimeStamp FROM pf_Feed WHERE UserID = @UserID ORDER BY TimeStamp DESC", itemCount))
				.AddParameter("@UserID", userID)
				.ExecuteReader()
				.ReadAll(r => list.Add(new FeedEvent { UserID = r.GetInt32(0), Message = r.GetString(1), Points = r.GetInt32(2), TimeStamp = r.GetDateTime(3)})));
			return list;
		}

		public List<FeedEvent> GetFeed(int itemCount)
		{
			var list = new List<FeedEvent>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(String.Format("SELECT TOP {0} UserID, Message, Points, TimeStamp FROM pf_Feed ORDER BY TimeStamp DESC", itemCount))
				.ExecuteReader()
				.ReadAll(r => list.Add(new FeedEvent { UserID = r.GetInt32(0), Message = r.GetString(1), Points = r.GetInt32(2), TimeStamp = r.GetDateTime(3) })));
			return list;
		}

		public void PublishEvent(int userID, string message, int points, DateTime timeStamp)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_Feed (UserID, Message, Points, TimeStamp) VALUES (@UserID, @Message, @Points, @TimeStamp)")
				.AddParameter("@UserID", userID)
				.AddParameter("@Message", message)
				.AddParameter("@Points", points)
				.AddParameter("@TimeStamp", timeStamp)
				.ExecuteNonQuery());
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
				connection.Command("DELETE FROM pf_Feed WHERE UserID = @UserID AND TimeStamp < @TimeStamp")
				.AddParameter("@UserID", userID)
				.AddParameter("@TimeStamp", timeCutOff)
				.ExecuteNonQuery());
		}
	}
}