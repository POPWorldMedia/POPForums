using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Services;
using PopForums.Services.Subscriptions;

namespace PopForums.AzureKit.Functions;

public class RenewalEnqueueProcessor
{
	private readonly IRenewalOrchestrationService _renewalOrchestrationService;
	private readonly IServiceHeartbeatService _serviceHeartbeatService;
	private readonly IErrorLog _errorLog;

	public RenewalEnqueueProcessor(IRenewalOrchestrationService renewalOrchestrationService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	{
		_renewalOrchestrationService = renewalOrchestrationService;
		_serviceHeartbeatService = serviceHeartbeatService;
		_errorLog = errorLog;
	}

	[Function("RenewalEnqueueProcessor")]
	public async Task Run([TimerTrigger("0 1 12 * * *")] TimerInfo myTimer, FunctionContext executionContext)
	{
		var logger = executionContext.GetLogger("AzureFunction");
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			await _renewalOrchestrationService.EnqueueUsersForRenewal();
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
			logger.LogError(exc, $"Exception thrown running {nameof(RenewalEnqueueProcessor)}");
		}

		stopwatch.Stop();
		logger.LogInformation($"C# Timer {nameof(RenewalEnqueueProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
		await _serviceHeartbeatService.RecordHeartbeat(typeof(RenewalEnqueueProcessor).FullName, "AzureFunction");
	}
}
