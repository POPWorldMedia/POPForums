using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public async Task SetForumRead(int userID, int forumID, DateTime readTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID; INSERT INTO pf_LastForumView (UserID, ForumID, LastForumViewDate)VALUES (@UserID, @ForumID, @LastForumViewDate)", new { UserID = userID, ForumID = forumID, LastForumViewDate = readTime }));
		}

		public async Task DeleteTopicReadsInForum(int userID, int forumID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE pf_LastTopicView FROM pf_LastTopicView JOIN pf_Topic ON pf_LastTopicView.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID AND pf_LastTopicView.UserID = @UserID", new { UserID = userID, ForumID = forumID }));
		}

		public async Task SetAllForumsRead(int userID, DateTime readTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_LastForumView WHERE UserID = @UserID; INSERT INTO pf_LastForumView SELECT @UserID, ForumID, @LastForumViewDate FROM pf_Forum", new { UserID = userID, LastForumViewDate = readTime }));
		}

		public async Task DeleteAllTopicReads(int userID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_LastTopicView WHERE UserID = @UserID", new { UserID = userID }));
		}

		public async Task SetTopicRead(int userID, int topicID, DateTime readTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_LastTopicView (UserID, TopicID, LastTopicViewDate) VALUES (@UserID, @TopicID, @LastTopicViewDate)", new { UserID = userID, TopicID = topicID, LastTopicViewDate = readTime }));
		}

		public async Task<Dictionary<int, DateTime>> GetLastReadTimesForForums(int userID)
		{
			Task<IEnumerable<KeyValuePair<int, DateTime>>> dictionary = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				dictionary = connection.QueryAsync<KeyValuePair<int, DateTime>>("SELECT ForumID AS [Key], LastForumViewDate AS [Value] FROM pf_LastForumView WHERE UserID = @UserID", new { UserID = userID }));
			return dictionary.Result.ToDictionary(p => p.Key, p => p.Value);
		}

		public async Task<DateTime?> GetLastReadTimesForForum(int userID, int forumID)
		{
			Task<DateTime?> lastRead = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				lastRead = connection.QuerySingleOrDefaultAsync<DateTime?>("SELECT LastForumViewDate FROM pf_LastForumView WHERE UserID = @UserID AND ForumID = @ForumID", new { UserID = userID, ForumID = forumID }));
			return lastRead.Result;
		}

		public async Task<Dictionary<int, DateTime>> GetLastReadTimesForTopics(int userID, IEnumerable<int> topicIDs)
		{
			var dictionary = new Dictionary<int, DateTime>();
			if (!topicIDs.Any())
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
			await _sqlObjectFactory.GetConnection().UsingAsync(async connection =>
			{
				var reader = await connection.ExecuteReaderAsync(sql, new {UserID = userID});
				while (reader.Read())
				{
					var key = reader.GetInt32(0);
					if (!dictionary.ContainsKey(key))
						dictionary.Add(key, reader.GetDateTime(1));
				}
			});
			return dictionary;
		}

		public async Task<DateTime?> GetLastReadTimeForTopic(int userID, int topicID)
		{
			Task<DateTime?> time = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				time = connection.QuerySingleOrDefaultAsync<DateTime?>("SELECT LastTopicViewDate FROM pf_LastTopicView WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
			return time.Result;
		}
	}
}