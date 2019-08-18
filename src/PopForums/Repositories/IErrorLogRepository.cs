using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IErrorLogRepository
	{
		Task<ErrorLogEntry> Create(DateTime timeStamp, string message, string stackTrace, string data, ErrorSeverity severity);
		Task<int> GetErrorCount();
		Task<List<ErrorLogEntry>> GetErrors(int startRow, int pageSize);
		Task DeleteError(int errorID);
		Task DeleteAllErrors();
	}
}