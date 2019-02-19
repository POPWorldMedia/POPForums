using System;
using System.Collections.Generic;
using System.Linq;
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

		public void CreateSession(int sessionID, int? userID, DateTime lastTime)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_UserSession (SessionID, UserID, LastTime) VALUES (@SessionID, @UserID, @LastTime)", new { SessionID = sessionID, UserID = userID, LastTime = lastTime }));
		}

		public bool UpdateSession(int sessionID, DateTime lastTime)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Execute("UPDATE pf_UserSession SET LastTime = @LastTime WHERE SessionID = @SessionID", new { SessionID = sessionID, LastTime = lastTime }) == 1);
			return result;
		}

		public bool IsSessionAnonymous(int sessionID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Query("SELECT UserID FROM pf_UserSession WHERE SessionID = @SessionID AND UserID IS NULL", new { SessionID = sessionID }).Any());
			return result;
		}

		public List<ExpiredUserSession> GetAndDeleteExpiredSessions(DateTime cutOffDate)
		{
			List<ExpiredUserSession> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<ExpiredUserSession>("SELECT SessionID, UserID, LastTime FROM pf_UserSession WHERE LastTime < @CutOff", new { CutOff = cutOffDate }).ToList());
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_UserSession WHERE LastTime < @CutOff", new { CutOff = cutOffDate }));
			return list;
		}

		public ExpiredUserSession GetSessionIDByUserID(int userID)
		{
			ExpiredUserSession session = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				session = connection.QuerySingleOrDefault<ExpiredUserSession>("SELECT SessionID, UserID, LastTime FROM pf_UserSession WHERE UserID = @UserID", new { UserID = userID }));
			return session;
		}

		public void DeleteSessions(int? userID, int sessionID)
		{
			if (userID.HasValue)
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Execute("DELETE FROM pf_UserSession WHERE SessionID = @SessionID OR UserID = @UserID", new { SessionID = sessionID, UserID = userID }));
			else
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Execute("DELETE FROM pf_UserSession WHERE SessionID = @SessionID", new { SessionID = sessionID }));
		}

		public int GetTotalSessionCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int>(CacheKeys.CurrentSessionCount);
			if (cacheObject != 0)
				return cacheObject;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_UserSession"));
			_cacheHelper.SetCacheObject(CacheKeys.CurrentSessionCount, count, 60);
			return count;
		}
	}
}
