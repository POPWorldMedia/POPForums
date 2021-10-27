using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.AzureKit.Queue;

public class SearchIndexQueueRepository : ISearchIndexQueueRepository
{
	private readonly IConfig _config;
	public const string QueueName = "pfsearchindexqueue";

	public SearchIndexQueueRepository(IConfig config)
	{
		_config = config;
	}

	public async Task Enqueue(SearchIndexPayload payload)
	{
		var serializedPayload = JsonSerializer.Serialize(payload);
		var client = await GetQueueClient();
		await client.SendMessageAsync(serializedPayload);
	}

#pragma warning disable 1998
	public async Task<SearchIndexPayload> Dequeue()
	{
		throw new System.NotImplementedException($"{nameof(Dequeue)} should never be called because it's automatically bound to an Azure function.");
	}
#pragma warning restore 1998

	private async Task<QueueClient> GetQueueClient()
	{
		var client = new QueueClient(_config.QueueConnectionString, QueueName, new QueueClientOptions
		{
			MessageEncoding = QueueMessageEncoding.Base64
		});
		await client.CreateIfNotExistsAsync();
		return client;
	}
}