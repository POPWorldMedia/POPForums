using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
    public class EmailProcessor
    {
	    private readonly IQueuedEmailMessageRepository _queuedEmailRepo;
	    private readonly ISmtpWrapper _smtpWrapper;
	    private readonly IServiceHeartbeatService _serviceHeartbeatService;
	    private readonly IErrorLog _errorLog;

	    public EmailProcessor(IQueuedEmailMessageRepository queuedEmailRepo, ISmtpWrapper smtpWrapper, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	    {
		    _queuedEmailRepo = queuedEmailRepo;
		    _smtpWrapper = smtpWrapper;
		    _serviceHeartbeatService = serviceHeartbeatService;
		    _errorLog = errorLog;
	    }

	    [Function("EmailProcessor")]
        public async Task RunAsync([QueueTrigger(PopForums.AzureKit.Queue.EmailQueueRepository.QueueName)]string jsonPayload, FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger("AzureFunction");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			QueuedEmailMessage message = null;
			try
			{
				var payload = JsonSerializer.Deserialize<EmailQueuePayload>(jsonPayload);
				message = _queuedEmailRepo.GetMessage(payload.MessageID).Result;
				if (payload.EmailQueuePayloadType == EmailQueuePayloadType.MassMessage)
				{
					message.ToEmail = payload.ToEmail;
					message.ToName = payload.ToName;
				}
				_smtpWrapper.Send(message);
			}
			catch (Exception exc)
			{
				if (message == null)
					_errorLog.Log(exc, ErrorSeverity.Email, "There was no message for the MailWorker to send.");
				else
					_errorLog.Log(exc, ErrorSeverity.Email, $"MessageID: {message.MessageID}, To: <{message.ToEmail}> {message.ToName}, Subject: {message.Subject}");
				logger.LogError(exc, $"Exception thrown running {nameof(EmailProcessor)}");
			}
			stopwatch.Stop();
			logger.LogInformation($"C# Queue {nameof(EmailProcessor)} function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			try
			{
				await _serviceHeartbeatService.RecordHeartbeat(typeof(EmailProcessor).FullName, "AzureFunction");
			}
			catch(Exception exc)
			{
				// we don't want to risk spamming anyone because of a database failure
				logger.LogError(exc, $"Logging the service heartbeat for {nameof(EmailProcessor)} failed.");
			}
		}
    }
}
