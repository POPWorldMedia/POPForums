using System;
using System.Collections.Generic;
using PopForums.Data.Sql;
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
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "INSERT INTO pf_SecurityLog (SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate) VALUES (@SecurityLogType, @UserID, @TargetUserID, @IP, @Message, @ActivityDate)")
				.AddParameter(_sqlObjectFactory, "@SecurityLogType", logEntry.SecurityLogType)
				.AddParameter(_sqlObjectFactory, "@UserID", logEntry.UserID.GetObjectOrDbNull())
				.AddParameter(_sqlObjectFactory, "@TargetUserID", logEntry.TargetUserID.GetObjectOrDbNull())
				.AddParameter(_sqlObjectFactory, "@IP", logEntry.IP)
				.AddParameter(_sqlObjectFactory, "@Message", logEntry.Message)
				.AddParameter(_sqlObjectFactory, "@ActivityDate", logEntry.ActivityDate)
				.ExecuteNonQuery());
		}

		public List<SecurityLogEntry> GetByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			var list = new List<SecurityLogEntry>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT SecurityLogID, SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate FROM pf_SecurityLog WHERE UserID = @UserID OR TargetUserID = @UserID AND ActivityDate >= @StartDate AND ActivityDate <= @EndDate ORDER BY ActivityDate")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@StartDate", startDate)
				.AddParameter(_sqlObjectFactory, "@EndDate", endDate)
				.ExecuteReader()
				.ReadAll(r => list.Add(new SecurityLogEntry
				                       	{
				                       		SecurityLogID = r.GetInt32(0),
											SecurityLogType = (SecurityLogType)r.GetInt32(1),
											UserID = r.NullIntDbHelper(2),
											TargetUserID = r.NullIntDbHelper(3),
											IP = r.GetString(4),
											Message = r.GetString(5),
											ActivityDate = r.GetDateTime(6)
				                       	})));
			return list;
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			var list = new List<IPHistoryEvent>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(_sqlObjectFactory, "SELECT SecurityLogID, ActivityDate, UserID, SecurityLogType, Message FROM pf_SecurityLog WHERE IP = @IP AND ActivityDate >= @Start AND ActivityDate <= @End ORDER BY ActivityDate")
				.AddParameter(_sqlObjectFactory, "@IP", ip)
				.AddParameter(_sqlObjectFactory, "@Start", start)
				.AddParameter(_sqlObjectFactory, "@End", end)
				.ExecuteReader()
				.ReadAll(r => list.Add(new IPHistoryEvent
				{
					ID = r.GetInt32(0),
					EventTime = r.GetDateTime(1),
					UserID = r.NullIntDbHelper(2),
					Name = String.Empty,
					Description = String.Format("{0} - {1}", ((SecurityLogType)r[3]).ToString(), r.NullStringDbHelper(4)),
					Type = typeof(SecurityLogEntry)
				})));
			return list;
		}
	}
}