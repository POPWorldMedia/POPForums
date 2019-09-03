using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IUserSessionRepository
	{
		Task CreateSession(int sessionID, int? userID, DateTime lastTime);
		Task<bool> UpdateSession(int sessionID, DateTime lastTime);
		Task<bool> IsSessionAnonymous(int sessionID);
		Task<List<ExpiredUserSession>> GetAndDeleteExpiredSessions(DateTime cutOffDate);
		Task<ExpiredUserSession> GetSessionIDByUserID(int userID);
		Task DeleteSessions(int? userID, int sessionID);
		Task<int> GetTotalSessionCount();
	}
}