using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
{
	public class ErrorLogRepository : IErrorLogRepository
	{
		public ErrorLogRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public ErrorLogEntry Create(DateTime timeStamp, string message, string stackTrace, string data, ErrorSeverity severity)
		{
			var errorID = 0;
			_sqlObjectFactory.GetConnection().Using(connection => errorID = Convert.ToInt32(connection.Command(_sqlObjectFactory, "INSERT INTO pf_ErrorLog (TimeStamp, Message, StackTrace, Data, Severity) VALUES (@TimeStamp, @Message, @StackTrace, @Data, @Severity)")
				.AddParameter(_sqlObjectFactory, "@TimeStamp", timeStamp)
				.AddParameter(_sqlObjectFactory, "@Message", message)
				.AddParameter(_sqlObjectFactory, "@StackTrace", stackTrace)
				.AddParameter(_sqlObjectFactory, "@Data", data)
				.AddParameter(_sqlObjectFactory, "@Severity", severity)
				.ExecuteAndReturnIdentity()));
			var errorLog = new ErrorLogEntry
			               	{
			               		ErrorID = errorID,
			               		TimeStamp = timeStamp,
			               		Message = message,
			               		StackTrace = stackTrace,
			               		Data = data,
			               		Severity = severity
			               	};
			return errorLog;
		}

		public int GetErrorCount()
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => count = Convert.ToInt32(
				connection.Command(_sqlObjectFactory, "SELECT COUNT(*) FROM pf_ErrorLog")
				.ExecuteScalar()));
			return count;
		}

		public List<ErrorLogEntry> GetErrors(int startRow, int pageSize)
		{
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY TimeStamp DESC)
AS Row, pf_ErrorLog.ErrorID, pf_ErrorLog.TimeStamp, pf_ErrorLog.Message, pf_ErrorLog.StackTrace, pf_ErrorLog.Data, pf_ErrorLog.Severity
FROM pf_ErrorLog)

SELECT ErrorID, TimeStamp, Message, StackTrace, Data, Severity
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			var logEntries = new List<ErrorLogEntry>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, "@StartRow", startRow)
					.AddParameter(_sqlObjectFactory, "@PageSize", pageSize)
					.ExecuteReader()
					.ReadAll(r => logEntries.Add(new ErrorLogEntry
					                             	{
					                             		ErrorID = r.GetInt32(0),
														TimeStamp = r.GetDateTime(1),
														Message = r.GetString(2),
														StackTrace = r.GetString(3),
														Data = r.GetString(4),
														Severity = (ErrorSeverity)r.GetInt32(5)
					                             	})));
			return logEntries;
		}

		public void DeleteError(int errorID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_ErrorLog WHERE ErrorID = @ErrorID")
				.AddParameter(_sqlObjectFactory, "@ErrorID", errorID)
				.ExecuteNonQuery());
		}

		public void DeleteAllErrors()
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_ErrorLog")
				.ExecuteNonQuery());
		}
	}
}