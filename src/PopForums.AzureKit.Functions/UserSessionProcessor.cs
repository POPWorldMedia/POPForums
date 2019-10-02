using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
    public static class UserSessionProcessor
    {
        [FunctionName("UserSessionProcessor")]
        public static async Task RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsAzureFunctionsAndQueues();

			var serviceProvider = services.BuildServiceProvider();
			var userSessionService = serviceProvider.GetService<IUserSessionService>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();
			var errorLog = serviceProvider.GetService<IErrorLog>();

			try
			{
				await userSessionService.CleanUpExpiredSessions();
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
				log.LogError(exc, $"Exception thrown running {nameof(UserSessionProcessor)}");
			}

			stopwatch.Stop();
			log.LogInformation($"C# Timer {nameof(UserSessionProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
            await serviceHeartbeatService.RecordHeartbeat(typeof(UserSessionProcessor).FullName, "AzureFunction");
		}
    }
}
