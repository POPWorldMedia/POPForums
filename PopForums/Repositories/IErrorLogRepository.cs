using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IErrorLogRepository
	{
		ErrorLogEntry Create(DateTime timeStamp, string message, string stackTrace, string data, ErrorSeverity severity);
		int GetErrorCount();
		List<ErrorLogEntry> GetErrors(int startRow, int pageSize);
		void DeleteError(int errorID);
		void DeleteAllErrors();
	}
}