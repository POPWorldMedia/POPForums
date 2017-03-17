using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class LoggerFactories
	{
		public static ILoggerFactory AddPopForumsLogger(this ILoggerFactory logger, IApplicationBuilder app)
		{
			var setupService = app.ApplicationServices.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return logger;
			var errorLog = app.ApplicationServices.GetService<IErrorLog>();
			var settingsManager = app.ApplicationServices.GetService<ISettingsManager>();
			var contextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
			logger.AddProvider(new LoggerProvider(errorLog, settingsManager, contextAccessor));
			return logger;
		}
	}
}