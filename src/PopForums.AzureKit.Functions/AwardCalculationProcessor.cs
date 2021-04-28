using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
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
		[Function("AwardCalculationProcessor")]
		public static async Task Run([QueueTrigger(AwardCalculationQueueRepository.QueueName)]
			string jsonPayload, ILogger log, IAwardCalculator awardCalculator, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

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