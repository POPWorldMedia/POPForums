using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
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
		internal const string TopicFields = "pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID";

		public async Task<Topic> GetLastUpdatedTopic(int forumID)
		{
			Task<Topic> topic = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topic = connection.QuerySingleOrDefaultAsync<Topic>("SELECT TOP 1 " + TopicFields + 
@" FROM pf_Topic WHERE pf_Topic.ForumID = @ForumID AND pf_Topic.IsDeleted = 0 
ORDER BY pf_Topic.LastPostTime DESC", new { ForumID = forumID }));
			return await topic;
		}

		public async Task<int> GetTopicCount(int forumID, bool includeDelete)
		{
			var sql = "SELECT COUNT(*) FROM pf_Topic WHERE ForumID = @ForumID";
			if (!includeDelete)
				sql += " AND IsDeleted = 0";
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				count = connection.ExecuteScalarAsync<int>(sql, new { ForumID = forumID }));
			return await count;
		}

		public async Task<int> GetTopicCountByUser(int userID, bool includeDeleted, List<int> excludedForums)
		{
			var sql = "SELECT COUNT(DISTINCT pf_Topic.TopicID) FROM pf_Topic JOIN pf_Post ON pf_Topic.TopicID = pf_Post.TopicID WHERE pf_Post.UserID = @UserID";
			if (!includeDeleted)
				sql += " AND pf_Topic.IsDeleted = 0 AND pf_Post.IsDeleted = 0";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>(sql, new { UserID = userID }));
			return await count;
		}

		public async Task<int> GetTopicCount(bool includeDeleted, List<int> excludedForums)
		{
			var sql = "SELECT COUNT(*) FROM pf_Topic";
			if (!includeDeleted)
				sql += " WHERE IsDeleted = 0";
			else
				sql += " WHERE 1 = 1";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>(sql));
			return await count;
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

		public async Task<int> GetPostCount(int forumID, bool includeDelete)
		{
			var sql = "SELECT COUNT(pf_Post.TopicID) FROM pf_Post JOIN pf_Topic ON pf_Post.TopicID = pf_Topic.TopicID WHERE pf_Topic.ForumID = @ForumID";
			if (!includeDelete)
				sql += " AND pf_Post.IsDeleted = 0 AND pf_Topic.IsDeleted = 0";
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>(sql, new { ForumID = forumID }));
			return await count;
		}

		public async Task<Topic> Get(int topicID)
		{
			Task<Topic> topic = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topic = connection.QuerySingleOrDefaultAsync<Topic>("SELECT " + TopicFields + " FROM pf_Topic WHERE TopicID = @TopicID", new { TopicID = topicID }));
			return await topic;
		}

		public async Task<Topic> Get(string urlName)
		{
			Task<Topic> topic = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topic = connection.QuerySingleOrDefaultAsync<Topic>("SELECT " + TopicFields + " FROM pf_Topic WHERE UrlName = @UrlName", new { UrlName = urlName }));
			return await topic;
		}

		public async Task<List<Topic>> Get(int forumID, bool includeDeleted, int startRow, int pageSize)
		{
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY IsPinned DESC, LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ForumID = @ForumID 
AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)))

SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<Topic>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				topics = connection.QueryAsync<Topic>(sql, new { ForumID = forumID, IncludeDeleted = includeDeleted, StartRow = startRow, PageSize = pageSize }));
			return topics.Result.ToList();
		}

		public async Task<List<Topic>> GetTopicsByUser(int userID, bool includeDeleted, List<int> excludedForums, int startRow, int pageSize)
		{
			var sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH FirstEntries AS (
SELECT ROW_NUMBER() OVER (PARTITION BY pf_Topic.TopicID ORDER BY IsPinned DESC, LastPostTime DESC)
AS GroupRow, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount,
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName,
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID
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
Entries.LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName, AnswerPostID
FROM Entries 
WHERE Row between
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<Topic>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topics = connection.QueryAsync<Topic>(sql, new { UserID = userID, IncludeDeleted = includeDeleted, StartRow = startRow, PageSize = pageSize }));
			return topics.Result.ToList();
		}

		public async Task<List<Topic>> Get(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize)
		{
			var sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			sql += @")
SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<Topic>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topics = connection.QueryAsync<Topic>(sql, new { IncludeDeleted = includeDeleted, StartRow = startRow, PageSize = pageSize }));
			return topics.Result.ToList();
		}

		public async Task<List<Topic>> Get(IEnumerable<int> topicIDs)
		{
			var list = topicIDs.ToList();
			if (list.Count == 0)
				return new List<Topic>();
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
			Task<IEnumerable<Topic>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topics = connection.QueryAsync<Topic>(sql));
			return topics.Result.ToList();
		}

		public async Task<List<Topic>> Get(int forumID, bool includeDeleted, List<int> excludedForums)
		{
			var sql = @"
SELECT pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic WHERE ForumID = @ForumID AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			Task<IEnumerable<Topic>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topics = connection.QueryAsync<Topic>(sql, new { ForumID = forumID, IncludeDeleted = includeDeleted }));
			return topics.Result.ToList();
		}

		public async Task<List<string>> GetUrlNamesThatStartWith(string urlName)
		{
			Task<IEnumerable<string>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<string>("SELECT UrlName FROM pf_Topic WHERE UrlName LIKE @UrlName + '%'", new { UrlName = urlName }));
			return list.Result.ToList();
		}

		public virtual async Task<int> Create(int forumID, string title, int replyCount, int viewCount, int startedByUserID, string startedByName, int lastPostUserID, string lastPostName, DateTime lastPostTime, bool isClosed, bool isPinned, bool isDeleted, string urlName)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QuerySingleAsync<int>("INSERT INTO pf_Topic (ForumID, Title, ReplyCount, ViewCount, StartedByUserID, StartedByName, LastPostUserID, LastPostName, LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName) VALUES (@ForumID, @Title, @ReplyCount, @ViewCount, @StartedByUserID, @StartedByName, @LastPostUserID, @LastPostName, @LastPostTime, @IsClosed, @IsPinned, @IsDeleted, @UrlName);SELECT CAST(SCOPE_IDENTITY() as int)", new { ForumID = forumID, Title = title, ReplyCount = replyCount, ViewCount = viewCount, StartedByUserID = startedByUserID, StartedByName = startedByName, LastPostUserID = lastPostUserID, LastPostName = lastPostName, LastPostTime = lastPostTime, IsClosed = isClosed, IsPinned = isPinned, IsDeleted = isDeleted, UrlName = urlName }));
			return await result;
		}

		public async Task IncrementReplyCount(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Topic SET ReplyCount = ReplyCount + 1 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task IncrementViewCount(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Topic SET ViewCount = ViewCount + 1 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task UpdateLastTimeAndUser(int topicID, int userID, string name, DateTime postTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET LastPostUserID = @LastPostUserID, LastPostName = @LastPostName, LastPostTime = @LastPostTime WHERE TopicID = @TopicID", new { LastPostUserID = userID, LastPostName = name, LastPostTime = postTime, TopicID = topicID }));
		}

		public async Task CloseTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Topic SET IsClosed = 1 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task OpenTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET IsClosed = 0 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task PinTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET IsPinned = 1 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task UnpinTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET IsPinned = 0 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task DeleteTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET IsDeleted = 1 WHERE TopicID = @TopicID", new { TopicID = topicID }));
			var key = string.Format(PostRepository.CacheKeys.PostPages, topicID);
			_cache.RemoveCacheObject(key);
		}

		public async Task UndeleteTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET IsDeleted = 0 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}

		public async Task HardDeleteTopic(int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_Topic WHERE TopicID = @TopicID", new { TopicID = topicID }));
			var key = string.Format(PostRepository.CacheKeys.PostPages, topicID);
			_cache.RemoveCacheObject(key);
		}

		public async Task UpdateTitleAndForum(int topicID, int forumID, string newTitle, string newUrlName)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET ForumID = @ForumID, Title = @Title, UrlName = @UrlName WHERE TopicID = @TopicID" ,new { ForumID = forumID, Title = newTitle, TopicID = topicID, UrlName = newUrlName}));
		}

		public async Task UpdateReplyCount(int topicID, int replyCount)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET ReplyCount = @ReplyCount WHERE TopicID = @TopicID", new { ReplyCount = replyCount, TopicID = topicID }));
		}

		public async Task<DateTime?> GetLastPostTime(int topicID)
		{
			Task<DateTime> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.ExecuteScalarAsync<DateTime>("SELECT LastPostTime FROM pf_Topic WHERE TopicID = @TopicID", new { TopicID = topicID }));
			return await result;
		}

		public async Task UpdateAnswerPostID(int topicID, int? postID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Topic SET AnswerPostID = @AnswerPostID WHERE TopicID = @TopicID", new { AnswerPostID = postID, TopicID = topicID }));
		}

		public async Task<IEnumerable<int>> CloseTopicsOlderThan(DateTime cutoffDate)
		{
			const string sql = @"DECLARE @ids TABLE (id int)
UPDATE pf_Topic SET IsClosed = 1 
OUTPUT INSERTED.TopicID
WHERE LastPostTime < @CutoffDate AND IsClosed = 0
SELECT id FROM @ids";
			Task<IEnumerable<int>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<int>(sql, new { CutoffDate = cutoffDate }));
			return await list;
		}

		public async Task<List<Tuple<string, DateTime>>> GetUrlNames(bool includeDeleted, List<int> excludedForums, int startRow, int pageSize)
		{
			var sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY LastPostTime DESC)
AS Row, pf_Topic.UrlName, pf_Topic.LastPostTime 
FROM pf_Topic WHERE ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0))";
			sql = GenerateExcludedForumSql(sql, excludedForums);
			sql += @")
SELECT UrlName AS Item1, LastPostTime AS Item2
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<Tuple<string, DateTime>>> topics = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				topics = connection.QueryAsync<Tuple<string, DateTime>>(sql, new { IncludeDeleted = includeDeleted, StartRow = startRow, PageSize = pageSize }));
			return topics.Result.ToList();
		}

	}
}