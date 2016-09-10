using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public static class LoggerFactories
	{
		public static ILoggerFactory AddPopForumsLogger(this ILoggerFactory logger, IApplicationBuilder app)
		{
			var errorLog = app.ApplicationServices.GetService<IErrorLog>();
			var settingsManager = app.ApplicationServices.GetService<ISettingsManager>();
			var contextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
			logger.AddProvider(new LoggerProvider(errorLog, settingsManager, contextAccessor));
			return logger;
		}
	}
}