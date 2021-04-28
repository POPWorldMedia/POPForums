using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
    public static class CloseAgedTopicsProcessor
    {
        [Function("CloseAgedTopicsProcessor")]
        public static async Task Run([TimerTrigger("0 0 */12 * * *")]TimerInfo myTimer, ILogger log, ITopicService topicService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				await topicService.CloseAgedTopics();
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
				log.LogError(exc, $"Exception thrown running {nameof(CloseAgedTopicsProcessor)}");
			}

			stopwatch.Stop();
			log.LogInformation($"C# Timer {nameof(CloseAgedTopicsProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
			await serviceHeartbeatService.RecordHeartbeat(typeof(CloseAgedTopicsProcessor).FullName, "AzureFunction");
		}
    }
}
