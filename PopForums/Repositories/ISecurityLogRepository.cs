using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISecurityLogRepository
	{
		void Create(SecurityLogEntry logEntry);
		List<SecurityLogEntry> GetByUserID(int userID, DateTime startDate, DateTime endDate);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
	}
}