using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.ElasticKit;
using PopForums.AzureKit.Queue;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
	public static class SearchIndexProcessor
	{
		[Function("SearchIndexProcessor")]
		public static async Task RunAsync([QueueTrigger(SearchIndexQueueRepository.QueueName)]
			string jsonPayload, ILogger log, ISearchIndexSubsystem searchIndexSubsystem, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			
			try
			{
				var payload = JsonSerializer.Deserialize<SearchIndexPayload>(jsonPayload);
				searchIndexSubsystem.DoIndex(payload.TopicID, payload.TenantID, payload.IsForRemoval);
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
				log.LogError(exc, $"Exception thrown running {nameof(SearchIndexProcessor)}");
			}

			stopwatch.Stop();
			log.LogInformation($"C# Queue SearchIndexProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			await serviceHeartbeatService.RecordHeartbeat(typeof(SearchIndexProcessor).FullName, "AzureFunction");
		}
	}
}