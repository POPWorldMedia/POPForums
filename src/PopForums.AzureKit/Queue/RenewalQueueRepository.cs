using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Queues;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Models.Subscriptions;
using PopForums.Repositories;

namespace PopForums.AzureKit.Queue;

public class RenewalQueueRepository : IRenewalQueueRepository
{
	private readonly IConfig _config;
	private readonly IErrorLog _errorLog;
	public const string QueueName = "pfrenewalqueue";

	public RenewalQueueRepository(IConfig config, IErrorLog errorLog)
	{
		_config = config;
		_errorLog = errorLog;
	}

	public async Task Enqueue(RenewalQueuePayload payload)
	{
		var serializedPayload = JsonSerializer.Serialize(payload);
		try
		{
			var client = await GetQueueClient();
			await client.SendMessageAsync(serializedPayload);
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Warning, $"Renewal enqueue failed on payload: {serializedPayload}");
		}
	}

#pragma warning disable 1998
	public async Task<RenewalQueuePayload> Dequeue()
	{
		throw new NotImplementedException($"{nameof(Dequeue)} should never be called because it's automatically bound to an Azure function.");
	}
#pragma warning restore 1998

	private async Task<QueueClient> GetQueueClient()
	{
		var options = new QueueClientOptions
		{
			MessageEncoding = QueueMessageEncoding.Base64,
			Retry = { NetworkTimeout = TimeSpan.FromSeconds(2), MaxRetries = 1, Mode = RetryMode.Fixed, Delay = TimeSpan.Zero, MaxDelay = TimeSpan.Zero }
		};
		var client = new QueueClient(_config.QueueConnectionString, QueueName, options);
		await client.CreateIfNotExistsAsync();
		return client;
	}
}
