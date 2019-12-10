using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.AwsKit;
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
		[FunctionName("SearchIndexProcessor")]
		public static async Task RunAsync([QueueTrigger(SearchIndexQueueRepository.QueueName)]
			string jsonPayload, ILogger log, ExecutionContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsAzureFunctionsAndQueues();
			services.AddTransient<IBroker, BrokerSink>();

			var serviceProvider = services.BuildServiceProvider();

			string searchType;
			var config = serviceProvider.GetService<IConfig>();
			switch (config.SearchProvider.ToLower())
			{
				case "elasticsearch":
					searchType = "ElasticSearch (PopForums.AwsKit)";
					services.AddPopForumsElasticSearch();
					break;
				case "azuresearch":
					searchType = "Azure Search (PopForums.AzureKit)";
					services.AddPopForumsAzureSearch();
					break;
				default:
					searchType = "Default (PopForums.Sql)";
					break;
			}

			serviceProvider = services.BuildServiceProvider();

			var searchIndexSubsystem = serviceProvider.GetService<ISearchIndexSubsystem>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();
			var errorLog = serviceProvider.GetService<IErrorLog>();

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
			log.LogInformation($"C# Queue SearchIndexProcessor ({searchType}) function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			await serviceHeartbeatService.RecordHeartbeat(typeof(SearchIndexProcessor).FullName, "AzureFunction");
		}
	}
}