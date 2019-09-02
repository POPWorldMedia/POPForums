using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task Create(SecurityLogEntry logEntry)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_SecurityLog (SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate) VALUES (@SecurityLogType, @UserID, @TargetUserID, @IP, @Message, @ActivityDate)", new { logEntry.SecurityLogType, logEntry.UserID, logEntry.TargetUserID, logEntry.IP, logEntry.Message, logEntry.ActivityDate }));
		}

		public async Task<List<SecurityLogEntry>> GetByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			Task<IEnumerable<SecurityLogEntry>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<SecurityLogEntry>("SELECT SecurityLogID, SecurityLogType, UserID, TargetUserID, IP, Message, ActivityDate FROM pf_SecurityLog WHERE (UserID = @UserID OR TargetUserID = @UserID) AND ActivityDate >= @StartDate AND ActivityDate <= @EndDate ORDER BY ActivityDate", new { UserID = userID, StartDate = startDate, EndDate = endDate }));
			return list.Result.ToList();
		}

		public async Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			var list = new List<IPHistoryEvent>();
			await _sqlObjectFactory.GetConnection().UsingAsync(async connection =>
			{
				var reader = await connection.ExecuteReaderAsync("SELECT SecurityLogID AS ID, ActivityDate AS EventTime, TargetUserID AS UserID, SecurityLogType, Message FROM pf_SecurityLog WHERE IP = @IP AND ActivityDate >= @Start AND ActivityDate <= @End ORDER BY ActivityDate", new {IP = ip, Start = start, End = end});
				while (reader.Read())
				{
					list.Add(new IPHistoryEvent
					{
						ID = reader.GetInt32(0),
						EventTime = reader.GetDateTime(1),
						UserID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
						Name = string.Empty,
						Description = $"{((SecurityLogType) reader[3]).ToString()} - {(reader.IsDBNull(4) ? null : reader.GetString(4))}",
						Type = "SecurityLogEntry"
					});
				}
			});
			return list;
		}
	}
}