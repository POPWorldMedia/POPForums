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
	public class SearchIndexProcessor
	{
		private readonly ISearchIndexSubsystem _searchIndexSubsystem;
		private readonly IServiceHeartbeatService _serviceHeartbeatService;
		private readonly IErrorLog _errorLog;

		public SearchIndexProcessor(ISearchIndexSubsystem searchIndexSubsystem, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
		{
			_searchIndexSubsystem = searchIndexSubsystem;
			_serviceHeartbeatService = serviceHeartbeatService;
			_errorLog = errorLog;
		}

		[Function("SearchIndexProcessor")]
		public async Task RunAsync([QueueTrigger(SearchIndexQueueRepository.QueueName)] string jsonPayload, FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger("AzureFunction");
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			
			try
			{
				var payload = JsonSerializer.Deserialize<SearchIndexPayload>(jsonPayload);
				_searchIndexSubsystem.DoIndex(payload.TopicID, payload.TenantID, payload.IsForRemoval);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				logger.LogError(exc, $"Exception thrown running {nameof(SearchIndexProcessor)}");
			}

			stopwatch.Stop();
			logger.LogInformation($"C# Queue SearchIndexProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			await _serviceHeartbeatService.RecordHeartbeat(typeof(SearchIndexProcessor).FullName, "AzureFunction");
		}
	}
}