using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PopForums.AzureKit.Queue;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Models.Subscriptions;
using PopForums.Services;
using PopForums.Services.Subscriptions;

namespace PopForums.AzureKit.Functions;

public class RenewalProcessor
{
	private readonly IRenewalOrchestrationService _renewalOrchestrationService;
	private readonly IServiceHeartbeatService _serviceHeartbeatService;
	private readonly IErrorLog _errorLog;

	public RenewalProcessor(IRenewalOrchestrationService renewalOrchestrationService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	{
		_renewalOrchestrationService = renewalOrchestrationService;
		_serviceHeartbeatService = serviceHeartbeatService;
		_errorLog = errorLog;
	}

	[Function("RenewalProcessor")]
	public async Task Run([QueueTrigger(RenewalQueueRepository.QueueName)] string jsonPayload, FunctionContext executionContext)
	{
		var logger = executionContext.GetLogger("AzureFunction");
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			var payload = JsonSerializer.Deserialize<RenewalQueuePayload>(jsonPayload);
			await _renewalOrchestrationService.ProcessRenewal(payload.UserID);
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
			logger.LogError(exc, $"Exception thrown running {nameof(RenewalProcessor)}");
		}

		stopwatch.Stop();
		logger.LogInformation($"C# Queue RenewalProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
		await _serviceHeartbeatService.RecordHeartbeat(typeof(RenewalProcessor).FullName, "AzureFunction");
	}
}
