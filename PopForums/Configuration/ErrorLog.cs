using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Configuration
{
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
			var message = String.Empty;
			var stackTrace = String.Empty;
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
				stackTrace = exception.StackTrace ?? String.Empty;
				foreach (DictionaryEntry item in exception.Data)
				{
					s.Append(item.Key);
					s.Append(": ");
					s.Append(item.Value);
					s.Append("\r\n");
				}
			}
			s.Append("\r\n");
			var context = HttpContext.Current;
			if (context != null)
			{
				if (context.User != null && context.User.Identity.IsAuthenticated)
				{
					var user = context.User as User;
					if (user != null)
					{
						s.Append("User: ");
						s.Append(user.Name);
						s.Append("\r\nE-mail: ");
						s.Append(user.Email);
						s.Append("\r\n\r\n");
					}
				}
				lock (context.Request.ServerVariables.AllKeys.SyncRoot)
				{
					foreach (var key in context.Request.ServerVariables.AllKeys)
					{
						if (context.Request.ServerVariables[key] != String.Empty && key != "ALL_HTTP")
						{
							s.Append(key);
							s.Append(": ");
							s.Append(context.Request.ServerVariables[key]);
							s.Append("\r\n");
						}
					}
				}
			}
			try
			{
				_errorLogRepository.Create(DateTime.UtcNow, message, stackTrace, s.ToString(), severity);
			}
			catch
			{
				throw new ErrorLogException(String.Format("Can't log error: {0}\r\n\r\n{1}\r\n\r\n{2}", message, stackTrace, s));
			}
		}

		public List<ErrorLogEntry> GetErrors(int pageIndex, int pageSize, out PagerContext pagerContext)
		{
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var errors = _errorLogRepository.GetErrors(startRow, pageSize);
			var errorCount = _errorLogRepository.GetErrorCount();
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(errorCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return errors;
		}

		public void DeleteError(int errorID)
		{
			_errorLogRepository.DeleteError(errorID);
		}

		public void DeleteAllErrors()
		{
			_errorLogRepository.DeleteAllErrors();
		}
	}
}