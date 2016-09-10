using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Web.Areas.Forums.Authorization;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public class Logger : ILogger
	{
		private readonly IErrorLog _errorLog;
		private readonly ISettingsManager _settingsManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public Logger(IErrorLog errorLog, ISettingsManager settingsManager, IHttpContextAccessor httpContextAccessor)
		{
			_errorLog = errorLog;
			_settingsManager = settingsManager;
			_httpContextAccessor = httpContextAccessor;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (IsEnabled(logLevel))
			{
				string data = null;
				if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
					data = "HttpContext not available";
				else
				{
					var context = _httpContextAccessor.HttpContext;
					if (context != null)
					{
						var s = new StringBuilder();
						var user =
							context.User.Identities.SingleOrDefault(
								x => x.AuthenticationType == PopForumsAuthorizationDefaults.AuthenticationScheme);
						if (user != null)
						{
							s.Append("User: ");
							s.Append(user.Name);
							s.Append("\r\n");
						}
						s.Append("IP: ");
						s.Append(context.Connection.RemoteIpAddress);
						s.Append("\r\n\r\n");
						foreach (var item in context.Request.Headers)
						{
							s.Append(item.Key);
							s.Append(": ");
							s.Append(item.Value);
							s.Append("\r\n");
						}
						data = s.ToString();
					}
				}
				_errorLog.Log(exception, ErrorSeverity.Error, data);
			}
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel == LogLevel.Error && _settingsManager.Current.LogErrors;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return NoopDisposable.Instance;
		}

		private class NoopDisposable : IDisposable
		{
			public static NoopDisposable Instance = new NoopDisposable();

			public void Dispose()
			{
			}
		}
	}
}