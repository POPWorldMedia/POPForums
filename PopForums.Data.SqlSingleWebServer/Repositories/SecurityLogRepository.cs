using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
				c.Command("INSERT INTO pf_SecurityLog (SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate) VALUES (@SecurityLogType, @UserID, @TargetUserID, @IP, @Message, @ActivityDate)")
				.AddParameter("@SecurityLogType", logEntry.SecurityLogType)
				.AddParameter("@UserID", logEntry.UserID.GetObjectOrDbNull())
				.AddParameter("@TargetUserID", logEntry.TargetUserID.GetObjectOrDbNull())
				.AddParameter("@IP", logEntry.IP)
				.AddParameter("@Message", logEntry.Message)
				.AddParameter("@ActivityDate", logEntry.ActivityDate)
				.ExecuteNonQuery());
		}

		public List<SecurityLogEntry> GetByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			var list = new List<SecurityLogEntry>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT SecurityLogID, SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate FROM pf_SecurityLog WHERE UserID = @UserID OR TargetUserID = @UserID AND ActivityDate >= @StartDate AND ActivityDate <= @EndDate ORDER BY ActivityDate")
				.AddParameter("@UserID", userID)
				.AddParameter("@StartDate", startDate)
				.AddParameter("@EndDate", endDate)
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
				c.Command("SELECT SecurityLogID, ActivityDate, UserID, SecurityLogType, Message FROM pf_SecurityLog WHERE IP = @IP AND ActivityDate >= @Start AND ActivityDate <= @End ORDER BY ActivityDate")
				.AddParameter("@IP", ip)
				.AddParameter("@Start", start)
				.AddParameter("@End", end)
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