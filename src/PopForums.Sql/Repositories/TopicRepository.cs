using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class TopicRepository : ITopicRepository
	{
		public TopicRepository(ISqlObjectFactory sqlObjectFactory, ICacheHelper cache)
		{
			_sqlObjectFactory = sqlObjectFactory;
			_cache = cache;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;
		private readonly ICacheHelper _cache;
		internal const string TopicFields = "pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID";

		public Topic GetLastUpdatedTopic(int forumID)
		{
			Topic topic = null;
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT TOP 1 " + TopicFields + 
@" FROM pf_Topic WHERE pf_Topic.ForumID = @ForumID AND pf_Topic.IsDeleted = 0 
ORDER BY pf_Topic.LastPostTime DESC")
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
				.ExecuteReader()
				.ReadOne(r => topic = GetTopicFromReader(r)));
			return topic;
		}

		public int GetTopicCount(int forumID, bool includeDelete)
		{
			var sql = "SELECT COUNT(*) FROM pf_Topic WHERE ForumID = @ForumID";
			if (!includeDelete)
				sql += " AND IsDeleted = 0";
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				count = Convert.ToInt32(connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
					.ExecuteScalar()));
			return count;
		}

		public int GetTopicCountByUser(int userID, bool includeDeleted, List<int> excludedForums)
		{
			var sql = "SELECT COUNT(DISTINCT pf_Topic.TopicID) FROM pf_Topic JOIN pf_Post ON pf_Topic.TopicID = pf_Post.TopicID WHERE pf_Post.UserID = @UserID";
			if (!includeDeleted)
				sql += " AND pf_Topic.IsDeleted = 0 AND pf_Post.IsDeleted = 0";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = Convert.ToInt32(connection.Command(_sqlObjectFactory, sql)
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.ExecuteScalar()));
			return count;
		}

		public int GetTopicCount(bool includeDeleted, List<int> excludedForums)
		{
			var sql = "SELECT COUNT(*) FROM pf_Topic";
			if (!includeDeleted)
				sql += " WHERE IsDeleted = 0";
			else
				sql += " WHERE 1 = 1";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = Convert.ToInt32(connection.Command(_sqlObjectFactory, sql)
					.ExecuteScalar()));
			return count;
		}

		private static string GenerateExcludedForumSql(string sql, List<int> excludedForums)
		{
			if (excludedForums.Count > 0)
			{
				sql += " AND ForumID NOT IN (";
				for (var i = 0; i < excludedForums.Count; i++)
				{
					sql += excludedForums[i];
					if (i != excludedForums.Count - 1)
						sql += ",";
				}
				sql += ")";
			}
			return sql;
		}

		public int GetPostCount(int forumID, bool includeDelete)
		{
			var sql = "SELECT COUNT(pf_Post.TopicID) FROM pf_Post JOIN pf_Topic ON pf_Post.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID";
			if (!includeDelete)
				sql += " AND pf_Post.IsDeleted = 0 AND pf_Topic.IsDeleted = 0";
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = Convert.ToInt32(connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
					.ExecuteScalar()));
			return count;
		}

		public Topic Get(int topicID)
		{
			Topic topic = null;
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT " + TopicFields + " FROM pf_Topic WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteReader()
				.ReadOne(r => topic = GetTopicFromReader(r)));
			return topic;
		}

		public Topic Get(string urlName)
		{
			Topic topic = null;
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT " + TopicFields + " FROM pf_Topic WHERE UrlName = @UrlName")
				.AddParameter(_sqlObjectFactory, "@UrlName", urlName)
				.ExecuteReader()
				.ReadOne(r => topic = GetTopicFromReader(r)));
			return topic;
		}

		public List<Topic> Get(int forumID, bool includeDeleted, int startRow, int pageSize)
		{
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY IsPinned DESC, LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ForumID = @ForumID 
AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)))

SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			var topics = new List<Topic>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
					.AddParameter(_sqlObjectFactory, "@IncludeDeleted", includeDeleted)
					.AddParameter(_sqlObjectFactory, "@StartRow", startRow)
					.AddParameter(_sqlObjectFactory, "@PageSize", pageSize)
					.ExecuteReader()
					.ReadAll(r => topics.Add(GetTopicFromReader(r))));
			return topics;
		}

		public List<Topic> GetTopicsByUser(int userID, bool includeDeleted, List<int> excludedForums, int startRow, int pageSize)
		{
			var sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH FirstEntries AS (
SELECT ROW_NUMBER() OVER (PARTITION BY pf_Topic.TopicID ORDER BY IsPinned DESC, LastPostTime DESC)
AS GroupRow, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount,
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName,
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID
FROM pf_Topic JOIN pf_Post ON pf_Topic.TopicID = pf_Post.TopicID
WHERE pf_Post.UserID = @UserID AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND pf_Topic.IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			sql += @"),

Entries as (
SELECT *,ROW_NUMBER() OVER (ORDER BY IsPinned DESC, LastPostTime DESC) AS Row
FROM   FirstEntries
WHERE  GroupRow = 1)

SELECT TopicID, Entries.ForumID, Entries.Title, ReplyCount, ViewCount,
StartedByUserID, StartedByName, LastPostUserID, Entries.LastPostName,
Entries.LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName, AnswerPostID
FROM Entries 
WHERE Row between
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			var topics = new List<Topic>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.AddParameter(_sqlObjectFactory, "@IncludeDeleted", includeDeleted)
					.AddParameter(_sqlObjectFactory, "@StartRow", startRow)
					.AddParameter(_sqlObjectFactory, "@PageSize", pageSize)
					.ExecuteReader()
					.ReadAll(r => topics.Add(GetTopicFromReader(r))));
			return topics;
		}

		public List<Topic> Get(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize)
		{
			var sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			sql += @")
SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			var topics = new List<Topic>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@IncludeDeleted", includeDeleted)
					.AddParameter(_sqlObjectFactory, "@StartRow", startRow)
					.AddParameter(_sqlObjectFactory, "@PageSize", pageSize)
					.ExecuteReader()
					.ReadAll(r => topics.Add(GetTopicFromReader(r))));
			return topics;
		}

		public List<Topic> Get(IEnumerable<int> topicIDs)
		{
			var list = topicIDs.ToList();
			var topics = new List<Topic>();
			var ids = string.Join(",", list);
			var sql = $@"SELECT {TopicFields} FROM pf_Topic 
WHERE TopicID IN ({ids})";
			var count = list.Count;
			if (count > 1)
			{
				var x = 1;
				var orderBy = @"
ORDER BY
CASE";
				foreach (var topicID in list.Take(count - 1))
				{
					orderBy += $@"
WHEN TopicID = {topicID} THEN {x}";
					x++;
				}
				orderBy += $@"
ELSE {x}
END";
				sql += orderBy;
			}
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.ExecuteReader()
					.ReadAll(r => topics.Add(GetTopicFromReader(r))));
			return topics;
		}

		public List<Topic> Get(int forumID, bool includeDeleted, List<int> excludedForums)
		{
			var sql = @"
SELECT pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ForumID = @ForumID AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			var topics = new List<Topic>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
					.AddParameter(_sqlObjectFactory, "@IncludeDeleted", includeDeleted)
					.ExecuteReader()
					.ReadAll(r => topics.Add(GetTopicFromReader(r))));
			return topics;
		}

		internal static Topic GetTopicFromReader(DbDataReader reader)
		{
			return new Topic
			    {
					TopicID = reader.GetInt32(0),
				    ForumID = reader.GetInt32(1),
				    Title = reader.GetString(2),
				    ReplyCount = reader.GetInt32(3),
				    ViewCount = reader.GetInt32(4),
				    StartedByUserID = reader.GetInt32(5),
				    StartedByName = reader.GetString(6),
				    LastPostUserID = reader.GetInt32(7),
				    LastPostName = reader.GetString(8),
				    LastPostTime = reader.GetDateTime(9),
				    IsClosed = reader.GetBoolean(10),
				    IsPinned = reader.GetBoolean(11),
				    IsDeleted = reader.GetBoolean(12),
				    IsIndexed = reader.GetBoolean(13),
				    UrlName = reader.GetString(14),
					AnswerPostID = reader.NullIntDbHelper(15)
			    };
		}

		public List<string> GetUrlNamesThatStartWith(string urlName)
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT UrlName FROM pf_Topic WHERE UrlName LIKE @UrlName + '%'")
				.AddParameter(_sqlObjectFactory, "@UrlName", urlName)
				.ExecuteReader()
				.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public virtual int Create(int forumID, string title, int replyCount, int viewCount, int startedByUserID, string startedByName, int lastPostUserID, string lastPostName, DateTime lastPostTime, bool isClosed, bool isPinned, bool isDeleted, bool isIndexed, string urlName)
		{
			object result = null;
			_sqlObjectFactory.GetConnection().Using(c =>
				result = c.Command(_sqlObjectFactory, "INSERT INTO pf_Topic (ForumID, Title, ReplyCount, ViewCount, StartedByUserID, StartedByName, LastPostUserID, LastPostName, LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName) VALUES (@ForumID, @Title, @ReplyCount, @ViewCount, @StartedByUserID, @StartedByName, @LastPostUserID, @LastPostName, @LastPostTime, @IsClosed, @IsPinned, @IsDeleted, @IsIndexed, @UrlName)")
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
				.AddParameter(_sqlObjectFactory, "@Title", title)
				.AddParameter(_sqlObjectFactory, "@ReplyCount", replyCount)
				.AddParameter(_sqlObjectFactory, "@ViewCount", viewCount)
				.AddParameter(_sqlObjectFactory, "@StartedByUserID", startedByUserID)
				.AddParameter(_sqlObjectFactory, "@StartedByName", startedByName)
				.AddParameter(_sqlObjectFactory, "@LastPostUserID", lastPostUserID)
				.AddParameter(_sqlObjectFactory, "@LastPostName", lastPostName)
				.AddParameter(_sqlObjectFactory, "@LastPostTime", lastPostTime)
				.AddParameter(_sqlObjectFactory, "@IsClosed", isClosed)
				.AddParameter(_sqlObjectFactory, "@IsPinned", isPinned)
				.AddParameter(_sqlObjectFactory, "@IsDeleted", isDeleted)
				.AddParameter(_sqlObjectFactory, "@IsIndexed", isIndexed)
				.AddParameter(_sqlObjectFactory, "@UrlName", urlName)
				.ExecuteAndReturnIdentity());
			return Convert.ToInt32(result);
		}

		public void IncrementReplyCount(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET ReplyCount = ReplyCount + 1 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void IncrementViewCount(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET ViewCount = ViewCount + 1 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void MarkTopicForIndexing(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsIndexed = 0 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "INSERT INTO pf_SearchQueue (TopicID) VALUES (@TopicID)")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void UpdateLastTimeAndUser(int topicID, int userID, string name, DateTime postTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET LastPostUserID = @LastPostUserID, LastPostName = @LastPostName, LastPostTime = @LastPostTime WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@LastPostUserID", userID)
				.AddParameter(_sqlObjectFactory, "@LastPostName", name)
				.AddParameter(_sqlObjectFactory, "@LastPostTime", postTime)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void CloseTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsClosed = 1 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void OpenTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsClosed = 0 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void PinTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsPinned = 1 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void UnpinTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsPinned = 0 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void DeleteTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsDeleted = 1 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
			var key = String.Format(PostRepository.CacheKeys.PostPages, topicID);
			_cache.RemoveCacheObject(key);
		}

		public void UndeleteTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET IsDeleted = 0 WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void HardDeleteTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "DELETE FROM pf_Topic WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
			var key = String.Format(PostRepository.CacheKeys.PostPages, topicID);
			_cache.RemoveCacheObject(key);
		}

		public void UpdateTitleAndForum(int topicID, int forumID, string newTitle, string newUrlName)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET ForumID = @ForumID, Title = @Title, UrlName = @UrlName WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@ForumID", forumID)
				.AddParameter(_sqlObjectFactory, "@Title", newTitle)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.AddParameter(_sqlObjectFactory, "@UrlName", newUrlName)
				.ExecuteNonQuery());
		}

		public void UpdateReplyCount(int topicID, int replyCount)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET ReplyCount = @ReplyCount WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@ReplyCount", replyCount)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public DateTime? GetLastPostTime(int topicID)
		{
			DateTime? result = null;
			_sqlObjectFactory.GetConnection().Using(connection => result = Convert.ToDateTime(connection.Command(_sqlObjectFactory, "SELECT LastPostTime FROM pf_Topic WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteScalar()));
			return result;
		}

		public void UpdateAnswerPostID(int topicID, int? postID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "UPDATE pf_Topic SET AnswerPostID = @AnswerPostID WHERE TopicID = @TopicID")
				.AddParameter(_sqlObjectFactory, "@AnswerPostID", postID.HasValue ? (object)postID.Value : DBNull.Value)
				.AddParameter(_sqlObjectFactory, "@TopicID", topicID)
				.ExecuteNonQuery());
		}
	}
}