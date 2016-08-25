using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IUserSessionRepository
	{
		void CreateSession(int sessionID, int? userID, DateTime lastTime);
		bool UpdateSession(int sessionID, DateTime lastTime);
		bool IsSessionAnonymous(int sessionID);
		List<ExpiredUserSession> GetAndDeleteExpiredSessions(DateTime cutOffDate);
		ExpiredUserSession GetSessionIDByUserID(int userID);
		void DeleteSessions(int? userID, int sessionID);
		int GetTotalSessionCount();
	}
}