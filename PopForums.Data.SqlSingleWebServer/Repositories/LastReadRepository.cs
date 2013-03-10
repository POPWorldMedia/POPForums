using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class LastReadRepository : ILastReadRepository
	{
		public LastReadRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public void SetForumRead(int userID, int forumID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID; INSERT INTO pf_LastForumView (UserID, ForumID, LastForumViewDate)VALUES (@UserID, @ForumID, @LastForumViewDate)")
				.AddParameter("@UserID", userID)
				.AddParameter("@ForumID", forumID)
				.AddParameter("@LastForumViewDate", readTime)
				.ExecuteNonQuery());
		}

		public void DeleteTopicReadsInForum(int userID, int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE pf_LastTopicView FROM pf_LastTopicView JOIN pf_Topic ON pf_LastTopicView.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID AND pf_LastTopicView.UserID = @UserID")
				.AddParameter("@UserID", userID)
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void SetAllForumsRead(int userID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_LastForumView WHERE UserID = @UserID; INSERT INTO pf_LastForumView SELECT @UserID, ForumID, @LastForumViewDate FROM pf_Forum")
				.AddParameter("@UserID", userID)
				.AddParameter("@LastForumViewDate", readTime)
				.ExecuteNonQuery());
		}

		public void DeleteAllTopicReads(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_LastTopicView WHERE UserID = @UserID")
				.AddParameter("@UserID", userID)
				.ExecuteNonQuery());
		}

		public void SetTopicRead(int userID, int topicID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID;")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.ExecuteNonQuery());
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_LastTopicView (UserID, TopicID, LastTopicViewDate) VALUES (@UserID, @TopicID, @LastTopicViewDate)")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.AddParameter("@LastTopicViewDate", readTime)
				.ExecuteNonQuery());
		}

		public Dictionary<int, DateTime> GetLastReadTimesForForums(int userID)
		{
			var dictionary = new Dictionary<int, DateTime>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT ForumID, LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID")
				.AddParameter("@UserID", userID)
				.ExecuteReader()
				.ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetDateTime(1))));
			return dictionary;
		}

		public DateTime? GetLastReadTimesForForum(int userID, int forumID)
		{
			DateTime? lastRead = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID")
				.AddParameter("@UserID", userID)
				.AddParameter("@ForumID", forumID)
				.ExecuteReader()
				.ReadOne(r => lastRead = r.GetDateTime(0)));
			return lastRead;
		}

		public Dictionary<int, DateTime> GetLastReadTimesForTopics(int userID, IEnumerable<int> topicIDs)
		{
			var dictionary = new Dictionary<int, DateTime>();
			if (topicIDs.Count() == 0)
				return dictionary;
			var inString = new StringBuilder();
			bool isFirst = true;
			foreach (var topicID in topicIDs)
			{
				if (!isFirst)
					inString.Append(", ");
				isFirst = false;
				inString.Append(topicID);
			}
			var sql = String.Format("SELECT TopicID, LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID IN ({0})", inString);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
				.AddParameter("@UserID", userID)
				.ExecuteReader()
				.ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetDateTime(1))));
			return dictionary;
		}

		public DateTime? GetLastReadTimeForTopic(int userID, int topicID)
		{
			DateTime? time = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.ExecuteReader()
				.ReadOne(r => time = r.GetDateTime(0)));
			return time;
		}
	}
}