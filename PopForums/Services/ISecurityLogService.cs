using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ISecurityLogService
	{
		List<SecurityLogEntry> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate);
		List<SecurityLogEntry> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate);
		void CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType);
		void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType);
		void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
	}
}