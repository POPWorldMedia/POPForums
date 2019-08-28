using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class PostRepository : IPostRepository
	{
		public PostRepository(ISqlObjectFactory sqlObjectFactory, ICacheHelper cache)
		{
			_sqlObjectFactory = sqlObjectFactory;
			_cache = cache;
		}

		public class CacheKeys
		{
			public const string PostPages = "PopForums.PostPages.{0}";
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;
		private readonly ICacheHelper _cache;
		private const string PostFields = "PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes";

		public virtual async Task<int> Create(int topicID, int parentPostID, string ip, bool isFirstInTopic, bool showSig, int userID, string name, string title, string fullText, DateTime postTime, bool isEdited, string lastEditName, DateTime? lastEditTime, bool isDeleted, int votes)
		{
			Task<int> postID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				postID = connection.QuerySingleAsync<int>("INSERT INTO pf_Post (TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes) VALUES (@TopicID, @ParentPostID, @IP, @IsFirstInTopic, @ShowSig, @UserID, @Name, @Title, @FullText, @PostTime, @IsEdited, @LastEditName, @LastEditTime, @IsDeleted, @Votes);SELECT CAST(SCOPE_IDENTITY() as int)", new { TopicID = topicID, ParentPostID = parentPostID, IP = ip, IsFirstInTopic = isFirstInTopic, ShowSig = showSig, UserID = userID, Name = name, Title = title, FullText = fullText, PostTime = postTime, IsEdited = isEdited, LastEditTime = lastEditTime, LastEditName = lastEditName, IsDeleted = isDeleted, Votes = votes }));
			var key = string.Format(CacheKeys.PostPages, topicID);
			_cache.RemoveCacheObject(key);
			return await postID;
		}

		public async Task<bool> Update(Post post)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.ExecuteAsync("UPDATE pf_Post SET TopicID = @TopicID, ParentPostID = @ParentPostID, IP = @IP, IsFirstInTopic = @IsFirstInTopic, ShowSig = @ShowSig, UserID = @UserID, Name = @Name, Title = @Title, FullText = @FullText, PostTime = @PostTime, IsEdited = @IsEdited, LastEditName = @LastEditName, LastEditTime = @LastEditTime, IsDeleted = @IsDeleted, Votes = @Votes WHERE PostID = @PostID", new { post.TopicID, post.ParentPostID, post.IP, post.IsFirstInTopic, post.ShowSig, post.UserID, post.Name, post.Title, post.FullText, post.PostTime, post.IsEdited, post.LastEditTime, post.LastEditName, post.IsDeleted, post.Votes, post.PostID }));
			var key = string.Format(CacheKeys.PostPages, post.TopicID);
			_cache.RemoveCacheObject(key);
			return result.Result == 1;
		}

		public async Task<List<Post>> Get(int topicID, bool includeDeleted, int startRow, int pageSize)
		{
			var key = string.Format(CacheKeys.PostPages, topicID);
			var page = startRow == 1 ? 1 : (startRow - 1) / pageSize + 1;
			if (!includeDeleted)
			{
				// we're only caching paged threads that do not include deleted posts, since only moderators
				// ever see threads that way, a small percentage of users
				var cachedList = _cache.GetPagedListCacheObject<Post>(key, page);
				if (cachedList != null)
					return cachedList;
			}
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY PostTime)
AS Row, PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes 
FROM pf_Post WHERE TopicID = @TopicID 
AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)))

SELECT PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<Post>> posts = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				posts = connection.QueryAsync<Post>(sql, new { TopicID = topicID, IncludeDeleted = includeDeleted, StartRow = startRow, PageSize = pageSize }));
			var list = posts.Result.ToList();
			if (!includeDeleted)
			{
				_cache.SetPagedListCacheObject(key, page, list);
			}
			return list;
		}

		public async Task<List<Post>> Get(int topicID, bool includeDeleted)
		{
			const string sql = "SELECT PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes FROM pf_Post WHERE TopicID = @TopicID AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)) ORDER BY PostTime";
			Task<IEnumerable<Post>> posts = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				posts = connection.QueryAsync<Post>(sql, new { TopicID = topicID, IncludeDeleted = includeDeleted }));
			return posts.Result.ToList();
		}

		public async Task<Post> GetLastInTopic(int topicID)
		{
			Task<Post> post = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				post = connection.QuerySingleOrDefaultAsync<Post>("SELECT TOP 1 " + PostFields + " FROM pf_Post WHERE TopicID = @TopicID AND IsDeleted = 0 ORDER BY PostTime DESC", new { TopicID = topicID}));
			return await post;
		}

		public async Task<int> GetReplyCount(int topicID, bool includeDeleted)
		{
			var sql = "SELECT COUNT(*) FROM pf_Post WHERE TopicID = @TopicID";
			if (!includeDeleted)
				sql += " AND IsDeleted = 0 AND IsFirstInTopic = 0";
			Task<int> replyCount = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				replyCount = connection.ExecuteScalarAsync<int>(sql, new { TopicID = topicID }));
			return await replyCount;
		}

		public async Task<Post> Get(int postID)
		{
			Task<Post> post = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				post = connection.QuerySingleOrDefaultAsync<Post>("SELECT " + PostFields + " FROM pf_Post WHERE PostID = @PostID", new { PostID = postID }));
			return await post;
		}

		public async Task<Dictionary<int, DateTime>> GetPostIDsWithTimes(int topicID, bool includeDeleted)
		{
			Task<IEnumerable<dynamic>> results = null;
			var sql = "SELECT PostID, PostTime FROM pf_Post WHERE TopicID = @TopicID";
			if (!includeDeleted)
				sql += " AND IsDeleted = 0";
			sql += " ORDER BY PostTime";
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				results = connection.QueryAsync(sql, new { TopicID = topicID }));
			var dictionary = results.Result.ToDictionary(r => (int) r.PostID, r => (DateTime) r.PostTime);
			return dictionary;
		}

		public int GetPostCount(int userID)
		{
			var postCount = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				postCount = connection.ExecuteScalar<int>("SELECT COUNT(PostID) FROM pf_Post JOIN pf_Topic ON pf_Post.TopicID = pf_Topic.TopicID WHERE pf_Post.UserID = @UserID AND pf_Post.IsDeleted = 0 AND pf_Topic.IsDeleted = 0", new { UserID = userID }));
			return postCount;
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			List<IPHistoryEvent> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<IPHistoryEvent>("SELECT PostID AS ID, PostTime AS EventTime, UserID, Name, Title AS Description FROM pf_Post WHERE IP = @IP AND PostTime >= @Start AND PostTime <= @End", new { IP = ip, Start = start, End = end }).ToList());
			foreach (var item in list)
				item.Type = "Post";
			return list;
		}

		public int GetLastPostID(int topicID)
		{
			const string sql = "SELECT PostID FROM pf_Post WHERE TopicID = @TopicID AND IsDeleted = 0 ORDER BY PostTime DESC";
			var id = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				id = connection.QuerySingleOrDefault<int>(sql, new { TopicID = topicID }));
			return id;
		}

		public int GetVoteCount(int postID)
		{
			const string sql = "SELECT Votes FROM pf_Post WHERE PostID = @PostID";
			var votes = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				votes = connection.QuerySingleOrDefault<int>(sql, new { PostID = postID }));
			return votes;
		}

		public int CalculateVoteCount(int postID)
		{
			const string sql = "SELECT COUNT(*) FROM pf_PostVote WHERE PostID = @PostID";
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				count = connection.ExecuteScalar<int>(sql, new { PostID = postID }));
			return count;
		}

		public void SetVoteCount(int postID, int votes)
		{
			const string sql = "UPDATE pf_Post SET Votes = @Votes WHERE PostID = @PostID";
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute(sql, new { Votes = votes, PostID = postID }));
		}

		public void VotePost(int postID, int userID)
		{
			const string sql = "INSERT INTO pf_PostVote (PostID, UserID) VALUES (@PostID, @UserID)";
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute(sql, new { PostID = postID, UserID = userID }));
		}

		public Dictionary<int, string> GetVotes(int postID)
		{
			Dictionary<int, string> results = null;
			const string sql = "SELECT V.UserID, U.Name FROM pf_PostVote V LEFT JOIN pf_PopForumsUser U ON V.UserID = U.UserID WHERE V.PostID = @PostID";
			_sqlObjectFactory.GetConnection().Using(connection =>
				results = connection.Query(sql, new { PostID = postID }).ToDictionary(r => (int)r.UserID, r => (string)r.Name));
			return results;
		}

		public List<int> GetVotedPostIDs(int userID, List<int> postIDs)
		{
			List<int> list = null;
			if (postIDs.Count == 0)
				return new List<int>();
			var inList = postIDs.Aggregate(string.Empty, (current, postID) => current + ("," + postID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = $"SELECT PostID FROM pf_PostVote WHERE PostID IN ({inList}) AND UserID = @UserID";
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<int>(sql, new { UserID = userID }).ToList());
			return list;
		}
	}
}
