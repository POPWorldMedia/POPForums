using System;
using System.Collections.Generic;
using System.Linq;
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

		public void Log(DateTime timeStamp, int userID, string userName, int moderationType, int? forumID, int topicID, int? postID, string comment, string oldText)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_ModerationLog (TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText) VALUES (@TimeStamp, @UserID, @UserName, @ModerationType, @ForumID, @TopicID, @PostID, @Comment, @OldText)", new { TimeStamp = timeStamp, UserID = userID, UserName = userName, ModerationType = moderationType, ForumID = forumID, TopicID = topicID, PostID = postID, Comment = comment.NullToEmpty(), OldText = oldText }));
		}

		public List<ModerationLogEntry> GetLog(DateTime start, DateTime end)
		{
			List<ModerationLogEntry> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<ModerationLogEntry>("SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TimeStamp >= @Start AND TimeStamp <= @End ORDER BY TimeStamp", new { Start = start, End = end }).ToList());
			return list;
		}

		public List<ModerationLogEntry> GetLog(int topicID, bool excludePostEntries)
		{
			List<ModerationLogEntry> list = null;
			var sql = "SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TopicID = @TopicID";
			if (excludePostEntries)
				sql += " AND PostID IS NULL";
			sql += "  ORDER BY TimeStamp";
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<ModerationLogEntry>(sql, new { TopicID = topicID }).ToList());
			return list;
		}

		public List<ModerationLogEntry> GetLog(int postID)
		{
			List<ModerationLogEntry> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<ModerationLogEntry>("SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE PostID = @PostID ORDER BY TimeStamp", new { PostID = postID }).ToList());
			return list;
		}
	}
}