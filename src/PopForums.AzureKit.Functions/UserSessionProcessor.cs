using System;
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
        public static void Run([TimerTrigger("*/1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
		{
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsAzureFunctionsAndQueues();

			var serviceProvider = services.BuildServiceProvider();
			var userSessionService = serviceProvider.GetService<IUserSessionService>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();

			userSessionService.CleanUpExpiredSessions();

			log.LogInformation($"C# Timer {nameof(UserSessionProcessor)} function executed at: {DateTime.UtcNow}");
            serviceHeartbeatService.RecordHeartbeat(typeof(UserSessionProcessor).FullName, "AzureFunction");
		}
    }
}
