using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Configuration
{
	public interface IErrorLog
	{
		void Log(Exception exception, ErrorSeverity severity);
		void Log(Exception exception, ErrorSeverity severity, string additionalContext);
		List<ErrorLogEntry> GetErrors(int pageIndex, int pageSize, out PagerContext pagerContext);
		PagedList<ErrorLogEntry> GetErrors(int pageIndex, int pageSize);
		Task DeleteError(int errorID);
		Task DeleteAllErrors();
	}

	public class ErrorLog : IErrorLog
	{
		public ErrorLog(IErrorLogRepository errorLogRepository)
		{
			_errorLogRepository = errorLogRepository;
		}

		private readonly IErrorLogRepository _errorLogRepository;

		public void Log(Exception exception, ErrorSeverity severity)
		{
			Log(exception, severity, null);
		}

		public void Log(Exception exception, ErrorSeverity severity, string additionalContext)
		{
			if (exception != null && exception is ErrorLogException)
				return;
			var message = string.Empty;
			var stackTrace = string.Empty;
			var s = new StringBuilder();
			if (additionalContext != null)
			{
				s.Append("Additional context:\r\n");
				s.Append(additionalContext);
				s.Append("\r\n\r\n");
			}
			if (exception != null)
			{
				message = exception.GetType().Name + ": " + exception.Message;
				if (exception.InnerException != null)
					message += "\r\n\r\nInner exception: " + exception.InnerException.Message;
				stackTrace = exception.StackTrace ?? string.Empty;
				foreach (DictionaryEntry item in exception.Data)
				{
					s.Append(item.Key);
					s.Append(": ");
					s.Append(item.Value);
					s.Append("\r\n");
				}
			}
			s.Append("\r\n");
			try
			{
				// TODO: Eventually make this async, but its web of call stacks are huge
				_errorLogRepository.Create(DateTime.UtcNow, message, stackTrace, s.ToString(), severity);
			}
			catch
			{
				throw new ErrorLogException($"Can't log error: {message}\r\n\r\n{stackTrace}\r\n\r\n{s}");
			}
		}

		public List<ErrorLogEntry> GetErrors(int pageIndex, int pageSize, out PagerContext pagerContext)
		{
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var errors = _errorLogRepository.GetErrors(startRow, pageSize).Result;
			var errorCount = _errorLogRepository.GetErrorCount().Result;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(errorCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return errors;
		}

		public PagedList<ErrorLogEntry> GetErrors(int pageIndex, int pageSize)
		{
			var errors = GetErrors(pageIndex, pageSize, out PagerContext pagerContext);
			var list = new PagedList<ErrorLogEntry> { PageCount = pagerContext.PageCount, PageIndex = pagerContext.PageIndex, PageSize = pagerContext.PageSize, List = errors };
			return list;
		}

		public async Task DeleteError(int errorID)
		{
			await _errorLogRepository.DeleteError(errorID);
		}

		public async Task DeleteAllErrors()
		{
			await _errorLogRepository.DeleteAllErrors();
		}
	}
}