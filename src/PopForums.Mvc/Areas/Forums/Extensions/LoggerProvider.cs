using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public class LoggerProvider : ILoggerProvider
	{
		private readonly IErrorLog _errorLog;
		private readonly ISettingsManager _settingsManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LoggerProvider(IErrorLog errorLog, ISettingsManager settingsManager, IHttpContextAccessor httpContextAccessor)
		{
			_errorLog = errorLog;
			_settingsManager = settingsManager;
			_httpContextAccessor = httpContextAccessor;
		}

		public void Dispose()
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new Logger(_errorLog, _settingsManager, _httpContextAccessor);
		}
	}
}