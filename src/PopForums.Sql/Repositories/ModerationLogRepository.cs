using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class ModerationLogRepository : IModerationLogRepository
	{
		public ModerationLogRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task Log(DateTime timeStamp, int userID, string userName, int moderationType, int? forumID, int topicID, int? postID, string comment, string oldText)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_ModerationLog (TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText) VALUES (@TimeStamp, @UserID, @UserName, @ModerationType, @ForumID, @TopicID, @PostID, @Comment, @OldText)", new { TimeStamp = timeStamp, UserID = userID, UserName = userName, ModerationType = moderationType, ForumID = forumID, TopicID = topicID, PostID = postID, Comment = comment.NullToEmpty(), OldText = oldText }));
		}

		public async Task<List<ModerationLogEntry>> GetLog(DateTime start, DateTime end)
		{
			Task<IEnumerable<ModerationLogEntry>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<ModerationLogEntry>("SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TimeStamp >= @Start AND TimeStamp <= @End ORDER BY TimeStamp", new { Start = start, End = end }));
			return list.Result.ToList();
		}

		public async Task<List<ModerationLogEntry>> GetLog(int topicID, bool excludePostEntries)
		{
			Task<IEnumerable<ModerationLogEntry>> list = null;
			var sql = "SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TopicID = @TopicID";
			if (excludePostEntries)
				sql += " AND PostID IS NULL";
			sql += "  ORDER BY TimeStamp";
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<ModerationLogEntry>(sql, new { TopicID = topicID }));
			return list.Result.ToList();
		}

		public async Task<List<ModerationLogEntry>> GetLog(int postID)
		{
			Task<IEnumerable<ModerationLogEntry>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<ModerationLogEntry>("SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE PostID = @PostID ORDER BY TimeStamp", new { PostID = postID }));
			return list.Result.ToList();
		}
	}
}