using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class SecurityLogRepository : ISecurityLogRepository
	{
		public SecurityLogRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public void Create(SecurityLogEntry logEntry)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_SecurityLog (SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate) VALUES (@SecurityLogType, @UserID, @TargetUserID, @IP, @Message, @ActivityDate)", new { logEntry.SecurityLogType, logEntry.UserID, logEntry.TargetUserID, logEntry.IP, logEntry.Message, logEntry.ActivityDate }));
		}

		public List<SecurityLogEntry> GetByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			List<SecurityLogEntry> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<SecurityLogEntry>("SELECT SecurityLogID, SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate FROM pf_SecurityLog WHERE UserID = @UserID OR TargetUserID = @UserID AND ActivityDate >= @StartDate AND ActivityDate <= @EndDate ORDER BY ActivityDate", new { UserID = userID, StartDate = startDate, EndDate = endDate }).ToList());
			return list;
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			List<IPHistoryEvent> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<IPHistoryEvent>("SELECT SecurityLogID, ActivityDate, UserID, SecurityLogType, Message FROM pf_SecurityLog WHERE IP = @IP AND ActivityDate >= @Start AND ActivityDate <= @End ORDER BY ActivityDate", new { IP = ip, Start = start, End = end }).ToList());
			return list;
		}
	}
}