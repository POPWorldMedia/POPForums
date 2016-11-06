using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID; INSERT INTO pf_LastForumView (UserID, ForumID, LastForumViewDate)VALUES (@UserID, @ForumID, @LastForumViewDate)")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
				.AddParameter(_sqlObjectFactory, "@LastForumViewDate", readTime)
				.ExecuteNonQuery());
		}

		public void DeleteTopicReadsInForum(int userID, int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE pf_LastTopicView FROM pf_LastTopicView JOIN pf_Topic ON pf_LastTopicView.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID AND pf_LastTopicView.UserID = @UserID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void SetAllForumsRead(int userID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_LastForumView WHERE UserID = @UserID; INSERT INTO pf_LastForumView SELECT @UserID, ForumID, @LastForumViewDate FROM pf_Forum")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@LastForumViewDate", readTime)
				.ExecuteNonQuery());
		}

		public void DeleteAllTopicReads(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_LastTopicView WHERE UserID = @UserID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.ExecuteNonQuery());
		}

		public void SetTopicRead(int userID, int topicID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID;")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_LastTopicView (UserID, TopicID, LastTopicViewDate) VALUES (@UserID, @TopicID, @LastTopicViewDate)")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.AddParameter(_sqlObjectFactory, "@LastTopicViewDate", readTime)
				.ExecuteNonQuery());
		}

		public Dictionary<int, DateTime> GetLastReadTimesForForums(int userID)
		{
			var dictionary = new Dictionary<int, DateTime>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT ForumID, LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.ExecuteReader()
				.ReadAll(r =>
				         {
					         var key = r.GetInt32(0);
							 if (!dictionary.ContainsKey(key))
								dictionary.Add(key, r.GetDateTime(1));
				         }));
			return dictionary;
		}

		public DateTime? GetLastReadTimesForForum(int userID, int forumID)
		{
			DateTime? lastRead = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
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
				connection.Command(_sqlObjectFactory, sql)
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.ExecuteReader()
				.ReadAll(r =>
						{
							var key = r.GetInt32(0);
							if (!dictionary.ContainsKey(key))
								dictionary.Add(key, r.GetDateTime(1));
				         }));
			return dictionary;
		}

		public DateTime? GetLastReadTimeForTopic(int userID, int topicID)
		{
			DateTime? time = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteReader()
				.ReadOne(r => time = r.GetDateTime(0)));
			return time;
		}
	}
}