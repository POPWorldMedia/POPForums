using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<ErrorLogEntry> Create(DateTime timeStamp, string message, string stackTrace, string data, ErrorSeverity severity)
		{
			Task<int> errorID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				errorID = connection.QuerySingleAsync<int>("INSERT INTO pf_ErrorLog (TimeStamp, Message, StackTrace, Data, Severity) VALUES (@TimeStamp, @Message, @StackTrace, @Data, @Severity);SELECT CAST(SCOPE_IDENTITY() as int)", new { TimeStamp = timeStamp, Message = message, StackTrace = stackTrace, Data = data, Severity = severity }));
			var errorLog = new ErrorLogEntry
			               	{
			               		ErrorID = errorID.Result,
			               		TimeStamp = timeStamp,
			               		Message = message,
			               		StackTrace = stackTrace,
			               		Data = data,
			               		Severity = severity
			               	};
			return errorLog;
		}

		public async Task<int> GetErrorCount()
		{
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pf_ErrorLog"));
			return await count;
		}

		public async Task<List<ErrorLogEntry>> GetErrors(int startRow, int pageSize)
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
			Task<IEnumerable<ErrorLogEntry>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<ErrorLogEntry>(sql, new { StartRow = startRow, PageSize = pageSize }));
			var logEntries = result.Result.ToList();
			return logEntries;
		}

		public async Task DeleteError(int errorID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_ErrorLog WHERE ErrorID = @ErrorID", new { ErrorID = errorID }));
		}

		public async Task DeleteAllErrors()
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("TRUNCATE TABLE pf_ErrorLog"));
		}
	}
}