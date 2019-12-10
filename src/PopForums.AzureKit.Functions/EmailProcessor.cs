using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
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
    public static class EmailProcessor
    {
        [FunctionName("EmailProcessor")]
        public static async Task RunAsync([QueueTrigger(PopForums.AzureKit.Queue.EmailQueueRepository.QueueName)]string jsonPayload, ILogger log, ExecutionContext context)
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
				var payload = JsonSerializer.Deserialize<EmailQueuePayload>(jsonPayload);
				message = queuedEmailRepo.GetMessage(payload.MessageID).Result;
				if (payload.EmailQueuePayloadType == EmailQueuePayloadType.MassMessage)
				{
					message.ToEmail = payload.ToEmail;
					message.ToName = payload.ToName;
				}
				smtpWrapper.Send(message);
			}
			catch (Exception exc)
			{
				if (message == null)
					errorLog.Log(exc, ErrorSeverity.Email, "There was no message for the MailWorker to send.");
				else
					errorLog.Log(exc, ErrorSeverity.Email, $"MessageID: {message.MessageID}, To: <{message.ToEmail}> {message.ToName}, Subject: {message.Subject}");
				log.LogError(exc, $"Exception thrown running {nameof(EmailProcessor)}");
			}
			stopwatch.Stop();
			log.LogInformation($"C# Queue {nameof(EmailProcessor)} function processed ({stopwatch.ElapsedMilliseconds}ms): {jsonPayload}");
			try
			{
				await serviceHeartbeatService.RecordHeartbeat(typeof(EmailProcessor).FullName, "AzureFunction");
			}
			catch(Exception exc)
			{
				// we don't want to risk spamming anyone because of a database failure
				log.LogError(exc, $"Logging the service heartbeat for {nameof(EmailProcessor)} failed.");
			}
		}
    }
}
