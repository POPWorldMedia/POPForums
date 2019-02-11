using System;
using System.Collections.Generic;
using System.Linq;
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

		public PrivateMessage Get(int pmID)
		{
			PrivateMessage pm = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				pm = connection.QuerySingleOrDefault<PrivateMessage>("SELECT PMID, Subject, LastPostTime, UserNames FROM pf_PrivateMessage WHERE PMID = @PMID", new { PMID = pmID }));
			return pm;
		}

		public List<PrivateMessagePost> GetPosts(int pmID)
		{
			List<PrivateMessagePost> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<PrivateMessagePost>("SELECT PMPostID, PMID, UserID, Name, PostTime, FullText FROM pf_PrivateMessagePost WHERE PMID = @PMID ORDER BY PostTime", new { PMID = pmID }).ToList());
			return list;
		}

		public virtual int CreatePrivateMessage(PrivateMessage pm)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				pm.PMID = connection.QuerySingle<int>("INSERT INTO pf_PrivateMessage (Subject, LastPostTime, UserNames) VALUES (@Subject, @LastPostTime, @UserNames);SELECT CAST(SCOPE_IDENTITY() as int)", new { pm.Subject, pm.LastPostTime, pm.UserNames }));
			return pm.PMID;
		}

		public void AddUsers(int pmID, List<int> userIDs, DateTime viewDate, bool isArchived)
		{
			foreach (var id in userIDs)
			{
				_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(id));
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Execute("INSERT INTO pf_PrivateMessageUser (PMID, UserID, LastViewDate, IsArchived) VALUES (@PMID, @UserID, @LastViewDate, @IsArchived)", new { PMID = pmID, UserID = id, LastViewDate = viewDate, IsArchived = isArchived }));
			}
		}

		public virtual int AddPost(PrivateMessagePost post)
		{
			var users = GetUsers(post.PMID);
			foreach (var user in users)
				_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(user.UserID));
			_sqlObjectFactory.GetConnection().Using(connection => 
				post.PMPostID = connection.QuerySingle<int>("INSERT INTO pf_PrivateMessagePost (PMID, UserID, Name, PostTime, FullText) VALUES (@PMID, @UserID, @Name, @PostTime, @FullText);SELECT CAST(SCOPE_IDENTITY() as int)", new { post.PMID, post.UserID, post.Name, post.PostTime, post.FullText }));
			return post.PMPostID;
		}

		public List<PrivateMessageUser> GetUsers(int pmID)
		{
			List<PrivateMessageUser> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<PrivateMessageUser>("SELECT PMID, UserID, LastViewDate, IsArchived FROM pf_PrivateMessageUser WHERE PMID = @PMID", new { PMID = pmID }).ToList());
			return list;
		}

		public void SetLastViewTime(int pmID, int userID, DateTime viewDate)
		{
			_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(userID));
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_PrivateMessageUser SET LastViewDate = @LastViewDate WHERE UserID = @UserID AND PMID = @PMID", new { LastViewDate = viewDate, UserID = userID, PMID = pmID }));
		}

		public void SetArchive(int pmID, int userID, bool isArchived)
		{
			_cacheHelper.RemoveCacheObject(CacheKeys.PMCount(userID));
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_PrivateMessageUser SET IsArchived = @IsArchived WHERE UserID = @UserID AND PMID = @PMID", new { IsArchived = isArchived, UserID = userID, PMID = pmID }));
		}

		public List<PrivateMessage> GetPrivateMessages(int userID, PrivateMessageBoxType boxType, int startRow, int pageSize)
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
			List<PrivateMessage> messsages = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				messsages = connection.Query<PrivateMessage>(sql, new { StartRow = startRow, PageSize = pageSize, UserID = userID, IsArchived = isArchived }).ToList());
			return messsages;
		}

		public int GetBoxCount(int userID, PrivateMessageBoxType boxType)
		{
			var isArchived = boxType == PrivateMessageBoxType.Archive;
			var sql = "SELECT COUNT(*) FROM pf_PrivateMessage P JOIN pf_PrivateMessageUser U ON P.PMID = U.PMID WHERE U.UserID = @UserID AND U.IsArchived = @IsArchived";
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>(sql, new { UserID = userID, IsArchived = isArchived }));
			return count;
		}

		public int GetUnreadCount(int userID)
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.PMCount(userID));
			if (cacheObject.HasValue)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				count = connection.ExecuteScalar<int>("SELECT COUNT(P.PMID) FROM pf_PrivateMessage P JOIN pf_PrivateMessageUser U ON P.PMID = U.PMID WHERE LastPostTime > LastViewDate AND U.UserID = @UserID AND U.IsArchived = 0", new { UserID = userID }));
			_cacheHelper.SetCacheObject(CacheKeys.PMCount(userID), count);
			return count;
		}

		public void UpdateLastPostTime(int pmID, DateTime lastPostTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				pmID = connection.Execute("UPDATE pf_PrivateMessage SET LastPostTime = @LastPostTime WHERE PMID = @PMID", new { LastPostTime = lastPostTime, PMID = pmID }));
		}
	}
}
