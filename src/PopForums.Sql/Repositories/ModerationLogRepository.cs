using System;
using System.Collections.Generic;
using System.Data.Common;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_ModerationLog (TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText) VALUES (@TimeStamp, @UserID, @UserName, @ModerationType, @ForumID, @TopicID, @PostID, @Comment, @OldText)")
					.AddParameter(_sqlObjectFactory, "@TimeStamp", timeStamp)
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.AddParameter(_sqlObjectFactory, "@UserName", userName)
					.AddParameter(_sqlObjectFactory, "@ModerationType", moderationType)
					.AddParameter(_sqlObjectFactory, "@ForumID", forumID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
					.AddParameter(_sqlObjectFactory, "@PostID", postID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@Comment", comment.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@OldText", oldText.GetObjectOrDbNull())
					.ExecuteNonQuery());
		}

		private static ModerationLogEntry PopulateFromReader(DbDataReader reader)
		{
			return new ModerationLogEntry
			       	{
			       		ModerationID = reader.GetInt32(0),
			       		TimeStamp = reader.GetDateTime(1),
			       		UserID = reader.GetInt32(2),
			       		UserName = reader.GetString(3),
			       		ModerationType = (ModerationType) reader.GetInt32(4),
			       		ForumID = reader.NullIntDbHelper(5),
			       		TopicID = reader.GetInt32(6),
			       		PostID = reader.NullIntDbHelper(7),
			       		Comment = reader.GetString(8),
			       		OldText = reader.NullStringDbHelper(9)
			       	};
		}

		public List<ModerationLogEntry> GetLog(DateTime start, DateTime end)
		{
			var list = new List<ModerationLogEntry>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TimeStamp >= @Start AND TimeStamp <= @End ORDER BY TimeStamp")
					.AddParameter(_sqlObjectFactory, "@Start", start)
					.AddParameter(_sqlObjectFactory, "@End", end)
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateFromReader(r))));
			return list;
		}

		public List<ModerationLogEntry> GetLog(int topicID, bool excludePostEntries)
		{
			var list = new List<ModerationLogEntry>();
			var sql = "SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE TopicID = @TopicID";
			if (excludePostEntries)
				sql += " AND PostID IS NULL";
			sql += "  ORDER BY TimeStamp";
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateFromReader(r))));
			return list;
		}

		public List<ModerationLogEntry> GetLog(int postID)
		{
			var list = new List<ModerationLogEntry>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT ModerationID, TimeStamp, UserID, UserName, ModerationType, ForumID, TopicID, PostID, Comment, OldText FROM pf_ModerationLog WHERE PostID = @PostID ORDER BY TimeStamp")
					.AddParameter(_sqlObjectFactory, "@PostID", postID)
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateFromReader(r))));
			return list;
		}
	}
}