using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
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
	        Config.SetPopForumsAppEnvironment(context.FunctionAppDirectory, "local.settings.json");
			var services = new ServiceCollection();
	        services.AddPopForumsBase();
	        services.AddPopForumsSql();
	        services.AddPopForumsAzureFunctionsAndQueues();
	        var serviceProvider = services.BuildServiceProvider();
			var queuedEmailRepo = serviceProvider.GetService<IQueuedEmailMessageRepository>();
			var smtpWrapper = serviceProvider.GetService<ISmtpWrapper>();
			var payload = JsonConvert.DeserializeObject<EmailQueuePayload>(jsonPayload);
			var message = queuedEmailRepo.GetMessage(payload.MessageID);
			smtpWrapper.Send(message);
			log.LogInformation($"C# Queue trigger function processed: {jsonPayload}");
        }
    }
}
