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
	public class PrivateMessageRepository : IPrivateMessageRepository
	{
		public PrivateMessageRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public class CacheKeys
		{
			public static string PMCount(int userID)
			{
				return "PopForums.PrivateMessages.Count." + userID;
			}
		}

		public async Task<PrivateMessage> Get(int pmID)
		{
			Task<PrivateMessage> pm = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				pm = connection.QuerySingleOrDefaultAsync<PrivateMessage>("SELECT PMID, Subject, LastPostTime, UserNames FROM pf_PrivateMessage WHERE PMID = @PMID", new { PMID = pmID }));
			return await pm;
		}

		public async Task<List<PrivateMessagePost>> GetPosts(int pmID)
		{
			Task<IEnumerable<PrivateMessagePost>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<PrivateMessagePost>("SELECT PMPostID, PMID, UserID, Name, PostTime, FullText FROM pf_PrivateMessagePost WHERE PMID = @PMID ORDER BY PostTime", new { PMID = pmID }));
			return result.Result.ToList();
		}

		public virtual async Task<int> CreatePrivateMessage(PrivateMessage pm)
		{
			Task<int> id = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				id = connection.QuerySingleAsync<int>("INSERT INTO pf_PrivateMessage (Subject, LastPostTime, UserNames) VALUES (@Subject, @LastPostTime, @UserNames);SELECT CAST(SCOPE_IDENTITY() as int)", new { pm.Subject, pm.LastPostTime, pm.UserNames }));
			pm.PMID = await id;
			return pm.PMID;
		}

		public async Task AddUsers(int pmID, List<int> userIDs, DateTime viewDate, bool isArchived)
		{
			foreach (var id in userIDs)
			{
				_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(id));
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					connection.ExecuteAsync("INSERT INTO pf_PrivateMessageUser (PMID, UserID, LastViewDate, IsArchived) VALUES (@PMID, @UserID, @LastViewDate, @IsArchived)", new { PMID = pmID, UserID = id, LastViewDate = viewDate, IsArchived = isArchived }));
			}
		}

		public virtual async Task<int> AddPost(PrivateMessagePost post)
		{
			var users = await GetUsers(post.PMID);
			foreach (var user in users)
				_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(user.UserID));
			Task<int> id = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				id = connection.QuerySingleAsync<int>("INSERT INTO pf_PrivateMessagePost (PMID, UserID, Name, PostTime, FullText) VALUES (@PMID, @UserID, @Name, @PostTime, @FullText);SELECT CAST(SCOPE_IDENTITY() as int)", new { post.PMID, post.UserID, post.Name, post.PostTime, post.FullText }));
			post.PMPostID = await id;
			return post.PMPostID;
		}

		public async Task<List<PrivateMessageUser>> GetUsers(int pmID)
		{
			Task<IEnumerable<PrivateMessageUser>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<PrivateMessageUser>("SELECT PMID, UserID, LastViewDate, IsArchived FROM pf_PrivateMessageUser WHERE PMID = @PMID", new { PMID = pmID }));
			return list.Result.ToList();
		}

		public async Task SetLastViewTime(int pmID, int userID, DateTime viewDate)
		{
			_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(userID));
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_PrivateMessageUser SET LastViewDate = @LastViewDate WHERE UserID = @UserID AND PMID = @PMID", new { LastViewDate = viewDate, UserID = userID, PMID = pmID }));
		}

		public async Task SetArchive(int pmID, int userID, bool isArchived)
		{
			_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(userID));
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_PrivateMessageUser SET IsArchived = @IsArchived WHERE UserID = @UserID AND PMID = @PMID", new { IsArchived = isArchived, UserID = userID, PMID = pmID }));
		}

		public async Task<List<PrivateMessage>> GetPrivateMessages(int userID, PrivateMessageBoxType boxType, int startRow, int pageSize)
		{
			var isArchived = boxType == PrivateMessageBoxType.Archive;
			const string sql =
			@"DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
	SELECT ROW_NUMBER() OVER (ORDER BY [LastPostTime] DESC)
	AS Row, P.PMID, Subject, LastPostTime, UserNames, U.LastViewDate 
	FROM pf_PrivateMessage P JOIN pf_PrivateMessageUser U 
	ON P.PMID = U.PMID WHERE U.UserID = @UserID 
	AND U.IsArchived = @IsArchived)

SELECT PMID, Subject, LastPostTime, UserNames, LastViewDate
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			Task<IEnumerable<PrivateMessage>> messsages = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				messsages = connection.QueryAsync<PrivateMessage>(sql, new { StartRow = startRow, PageSize = pageSize, UserID = userID, IsArchived = isArchived }));
			return messsages.Result.ToList();
		}

		public async Task<int> GetBoxCount(int userID, PrivateMessageBoxType boxType)
		{
			var isArchived = boxType == PrivateMessageBoxType.Archive;
			var sql = "SELECT COUNT(*) FROM pf_PrivateMessage P JOIN pf_PrivateMessageUser U ON P.PMID = U.PMID WHERE U.UserID = @UserID AND U.IsArchived = @IsArchived";
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>(sql, new { UserID = userID, IsArchived = isArchived }));
			return await count;
		}

		public async Task<int> GetUnreadCount(int userID)
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.PMCount(userID));
			if (cacheObject.HasValue)
				return cacheObject.Value;
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(P.PMID) FROM pf_PrivateMessage P JOIN pf_PrivateMessageUser U ON P.PMID = U.PMID WHERE LastPostTime > LastViewDate AND U.UserID = @UserID AND U.IsArchived = 0", new { UserID = userID }));
			_cacheHelper.SetCacheObject(CacheKeys.PMCount(userID), count.Result);
			return await count;
		}

		public async Task UpdateLastPostTime(int pmID, DateTime lastPostTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_PrivateMessage SET LastPostTime = @LastPostTime WHERE PMID = @PMID", new { LastPostTime = lastPostTime, PMID = pmID }));
		}
	}
}
