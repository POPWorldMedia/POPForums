using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
    public static class CloseAgedTopicsProcessor
    {
        [FunctionName("CloseAgedTopicsProcessor")]
        public static void Run([TimerTrigger("0 0 */12 * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsAzureFunctionsAndQueues();

			var serviceProvider = services.BuildServiceProvider();
			var topicService = serviceProvider.GetService<ITopicService>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();
			var errorLog = serviceProvider.GetService<IErrorLog>();

			try
			{
				topicService.CloseAgedTopics();
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}

			stopwatch.Stop();
			log.LogInformation($"C# Timer {nameof(CloseAgedTopicsProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
			serviceHeartbeatService.RecordHeartbeat(typeof(CloseAgedTopicsProcessor).FullName, "AzureFunction");
		}
    }
}
