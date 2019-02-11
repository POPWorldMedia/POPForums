using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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
			_sqlObjectFactory.GetConnection().Using(connection => 
				errorID = connection.QuerySingle<int>("INSERT INTO pf_ErrorLog (TimeStamp, Message, StackTrace, Data, Severity) VALUES (@TimeStamp, @Message, @StackTrace, @Data, @Severity);SELECT CAST(SCOPE_IDENTITY() as int)", new { TimeStamp = timeStamp, Message = message, StackTrace = stackTrace, Data = data, Severity = severity }));
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
			_sqlObjectFactory.GetConnection().Using(connection => 
				count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_ErrorLog"));
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
			List<ErrorLogEntry> logEntries = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				logEntries = connection.Query<ErrorLogEntry>(sql, new { StartRow = startRow, PageSize = pageSize }).ToList());
			return logEntries;
		}

		public void DeleteError(int errorID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_ErrorLog WHERE ErrorID = @ErrorID", new { ErrorID = errorID }));
		}

		public void DeleteAllErrors()
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("TRUNCATE TABLE pf_ErrorLog"));
		}
	}
}