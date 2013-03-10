using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Configuration
{
	public interface IErrorLog
	{
		void Log(Exception exception, ErrorSeverity severity);
		void Log(Exception exception, ErrorSeverity severity, string additionalContext);
		List<ErrorLogEntry> GetErrors(int pageIndex, int pageSize, out PagerContext pagerContext);
		void DeleteError(int errorID);
		void DeleteAllErrors();
	}
}