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
	public class AwardCalculationProcessor
	{
		private readonly IAwardCalculator _awardCalculator;
		private readonly IServiceHeartbeatService _serviceHeartbeatService;
		private readonly IErrorLog _errorLog;

		public AwardCalculationProcessor(IAwardCalculator awardCalculator, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
		{
			_awardCalculator = awardCalculator;
			_serviceHeartbeatService = serviceHeartbeatService;
			_errorLog = errorLog;
		}

		[Function("AwardCalculationProcessor")]
		public async Task Run([QueueTrigger(AwardCalculationQueueRepository.QueueName)] string jsonPayload, FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger("AzureFunction");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				var payload = JsonSerializer.Deserialize<AwardCalculationPayload>(jsonPayload);
				await _awardCalculator.ProcessCalculation(payload.EventDefinitionID, payload.UserID);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				logger.LogError(exc, $"Exception thrown running {nameof(AwardCalculationProcessor)}");
			}

			stopwatch.Stop();
			logger.LogInformation($"C# Queue AwardCalculationProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			await _serviceHeartbeatService.RecordHeartbeat(typeof(AwardCalculationProcessor).FullName, "AzureFunction");
		}
	}
}