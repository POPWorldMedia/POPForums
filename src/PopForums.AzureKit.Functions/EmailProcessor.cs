using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Functions
{
    public static class EmailProcessor
    {
        [FunctionName("EmailProcessor")]
        public static void Run([QueueTrigger(PopForums.AzureKit.Queue.EmailQueueRepository.QueueName)]string jsonPayload, ILogger log, ExecutionContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
	        services.AddPopForumsBase();
	        services.AddPopForumsSql();
	        services.AddPopForumsAzureFunctionsAndQueues();

	        var serviceProvider = services.BuildServiceProvider();
			var queuedEmailRepo = serviceProvider.GetService<IQueuedEmailMessageRepository>();
			var smtpWrapper = serviceProvider.GetService<ISmtpWrapper>();
			var serviceHeartbeatService = serviceProvider.GetService<IServiceHeartbeatService>();
			var errorLog = serviceProvider.GetService<IErrorLog>();

			QueuedEmailMessage message = null;
			try
			{
				var payload = JsonConvert.DeserializeObject<EmailQueuePayload>(jsonPayload);
				message = queuedEmailRepo.GetMessage(payload.MessageID);
				smtpWrapper.Send(message);
			}
			catch (Exception exc)
			{
				if (message == null)
					errorLog.Log(exc, ErrorSeverity.Email, "There was no message for the MailWorker to send.");
				else
					errorLog.Log(exc, ErrorSeverity.Email, $"MessageID: {message.MessageID}, To: <{message.ToEmail}> {message.ToName}, Subject: {message.Subject}");
			}
			stopwatch.Stop();
			log.LogInformation($"C# Queue {nameof(EmailProcessor)} function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			try
			{
				serviceHeartbeatService.RecordHeartbeat(typeof(EmailProcessor).FullName, "AzureFunction");
			}
			catch(Exception exc)
			{
				// we don't want to risk spamming anyone because of a database failure
				log.LogError(exc, $"Logging the service heartbeat for {nameof(EmailProcessor)} failed.");
			}
		}
    }
}
