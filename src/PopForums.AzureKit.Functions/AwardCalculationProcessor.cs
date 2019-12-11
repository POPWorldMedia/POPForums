using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.AzureKit.Queue;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
	public static class AwardCalculationProcessor
	{
		[FunctionName("AwardCalculationProcessor")]
		public static async Task Run([QueueTrigger(AwardCalculationQueueRepository.QueueName)]
			string jsonPayload, ILogger log, ExecutionContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsAzureFunctionsAndQueues();

			var serviceProvider = services.BuildServiceProvider();
			var awardCalculator = serviceProvider.GetService<IAwardCalculator>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();
			var errorLog = serviceProvider.GetService<IErrorLog>();

			try
			{
				var payload = JsonSerializer.Deserialize<AwardCalculationPayload>(jsonPayload);
				await awardCalculator.ProcessCalculation(payload.EventDefinitionID, payload.UserID);
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
				log.LogError(exc, $"Exception thrown running {nameof(AwardCalculationProcessor)}");
			}

			stopwatch.Stop();
			log.LogInformation($"C# Queue AwardCalculationProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			await serviceHeartbeatService.RecordHeartbeat(typeof(AwardCalculationProcessor).FullName, "AzureFunction");
		}
	}
}