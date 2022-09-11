using Microsoft.Azure.Functions.Worker;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using PopForums.AzureKit.Queue;
using PopForums.Messaging;

namespace PopForums.AzureKit.Functions;

public class SubscribeNotificationProcessor
{
	private readonly ISubscribedTopicsService _subscribedTopicsService;
	private readonly IServiceHeartbeatService _serviceHeartbeatService;
	private readonly IErrorLog _errorLog;
	private readonly INotificationTunnel _notificationTunnel;

	public SubscribeNotificationProcessor(ISubscribedTopicsService subscribedTopicsService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog, INotificationTunnel notificationTunnel)
	{
		_subscribedTopicsService = subscribedTopicsService;
		_serviceHeartbeatService = serviceHeartbeatService;
		_errorLog = errorLog;
		_notificationTunnel = notificationTunnel;
	}

	[Function("SubscribeNotificationProcessor")]
	public async Task Run([QueueTrigger(SubscribeNotificationRepository.QueueName)] string jsonPayload, FunctionContext executionContext)
	{
		var logger = executionContext.GetLogger("AzureFunction");
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			var payload = JsonSerializer.Deserialize<SubscribeNotificationPayload>(jsonPayload);
			var userIDs = await _subscribedTopicsService.GetSubscribedUserIDs(payload.TopicID);
			var filteredUserIDs = userIDs.Where(x => x != payload.PostingUserID);
			foreach (var userID in filteredUserIDs)
				_notificationTunnel.SendNotificationForReply(payload.PostingUserName, payload.TopicTitle, payload.TopicID, userID, payload.TenantID);
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
			logger.LogError(exc, $"Exception thrown running {nameof(SubscribeNotificationProcessor)}");
		}

		stopwatch.Stop();
		logger.LogInformation($"C# Queue SubscribeNotificationProcessor function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
		await _serviceHeartbeatService.RecordHeartbeat(typeof(SubscribeNotificationProcessor).FullName, "AzureFunction");
	}
}