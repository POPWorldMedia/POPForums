using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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
				connection.Execute("DELETE FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID; INSERT INTO pf_LastForumView (UserID, ForumID, LastForumViewDate)VALUES (@UserID, @ForumID, @LastForumViewDate)", new { UserID = userID, ForumID = forumID, LastForumViewDate = readTime }));
		}

		public void DeleteTopicReadsInForum(int userID, int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE pf_LastTopicView FROM pf_LastTopicView JOIN pf_Topic ON pf_LastTopicView.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID AND pf_LastTopicView.UserID = @UserID", new { UserID = userID, ForumID = forumID }));
		}

		public void SetAllForumsRead(int userID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_LastForumView WHERE UserID = @UserID; INSERT INTO pf_LastForumView SELECT @UserID, ForumID, @LastForumViewDate FROM pf_Forum", new { UserID = userID, LastForumViewDate = readTime }));
		}

		public void DeleteAllTopicReads(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_LastTopicView WHERE UserID = @UserID", new { UserID = userID }));
		}

		public void SetTopicRead(int userID, int topicID, DateTime readTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_LastTopicView (UserID, TopicID, LastTopicViewDate) VALUES (@UserID, @TopicID, @LastTopicViewDate)", new { UserID = userID, TopicID = topicID, LastTopicViewDate = readTime }));
		}

		public Dictionary<int, DateTime> GetLastReadTimesForForums(int userID)
		{
			Dictionary<int, DateTime> dictionary = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				dictionary = connection.Query<KeyValuePair<int, DateTime>>("SELECT ForumID AS [Key], LastForumViewDate AS [Value] FROM pf_LastForumView WHERE UserID = @UserID", new { UserID = userID }).ToDictionary(p => p.Key, p => p.Value));
			return dictionary;
		}

		public DateTime? GetLastReadTimesForForum(int userID, int forumID)
		{
			DateTime? lastRead = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				lastRead = connection.QuerySingleOrDefault<DateTime?>("SELECT LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID", new { UserID = userID, ForumID = forumID }));
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
			var sql = $"SELECT TopicID, LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID IN ({inString})";
			_sqlObjectFactory.GetConnection().Using(connection =>
			{
				var reader = connection.ExecuteReader(sql, new {UserID = userID});
				while (reader.Read())
				{
					var key = reader.GetInt32(0);
					if (!dictionary.ContainsKey(key))
						dictionary.Add(key, reader.GetDateTime(1));
				}
			});
			return dictionary;
		}

		public DateTime? GetLastReadTimeForTopic(int userID, int topicID)
		{
			DateTime? time = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				time = connection.QuerySingleOrDefault<DateTime?>("SELECT LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
			return time;
		}
	}
}