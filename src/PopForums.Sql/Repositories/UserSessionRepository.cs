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
	public class UserSessionRepository : IUserSessionRepository
	{
		public UserSessionRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public class CacheKeys
		{
			public const string CurrentSessionCount = "PopForums.Session.CurrentCount";
		}

		public async Task CreateSession(int sessionID, int? userID, DateTime lastTime)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_UserSession (SessionID, UserID, LastTime) VALUES (@SessionID, @UserID, @LastTime)", new { SessionID = sessionID, UserID = userID, LastTime = lastTime }));
		}

		public async Task<bool> UpdateSession(int sessionID, DateTime lastTime)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.ExecuteAsync("UPDATE pf_UserSession SET LastTime = @LastTime WHERE SessionID = @SessionID", new { SessionID = sessionID, LastTime = lastTime }));
			return result.Result == 1;
		}

		public async Task<bool> IsSessionAnonymous(int sessionID)
		{
			Task<IEnumerable<int>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<int>("SELECT UserID FROM pf_UserSession WHERE SessionID = @SessionID AND UserID IS NULL", new { SessionID = sessionID }));
			return result.Result.Any();
		}

		public async Task<List<ExpiredUserSession>> GetAndDeleteExpiredSessions(DateTime cutOffDate)
		{
			Task<IEnumerable<ExpiredUserSession>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<ExpiredUserSession>("SELECT SessionID, UserID, LastTime FROM pf_UserSession WHERE LastTime < @CutOff", new { CutOff = cutOffDate }));
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_UserSession WHERE LastTime < @CutOff", new { CutOff = cutOffDate }));
			return list.Result.ToList();
		}

		public async Task<ExpiredUserSession> GetSessionIDByUserID(int userID)
		{
			Task<ExpiredUserSession> session = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				session = connection.QueryFirstOrDefaultAsync<ExpiredUserSession>("SELECT SessionID, UserID, LastTime FROM pf_UserSession WHERE UserID = @UserID", new { UserID = userID }));
			return await session;
		}

		public async Task DeleteSessions(int? userID, int sessionID)
		{
			if (userID.HasValue)
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					connection.ExecuteAsync("DELETE FROM pf_UserSession WHERE SessionID = @SessionID OR UserID = @UserID", new { SessionID = sessionID, UserID = userID }));
			else
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					connection.ExecuteAsync("DELETE FROM pf_UserSession WHERE SessionID = @SessionID", new { SessionID = sessionID }));
		}

		public async Task<int> GetTotalSessionCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int>(CacheKeys.CurrentSessionCount);
			if (cacheObject != 0)
				return cacheObject;
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pf_UserSession"));
			_cacheHelper.SetCacheObject(CacheKeys.CurrentSessionCount, count.Result, 60);
			return count.Result;
		}
	}
}
