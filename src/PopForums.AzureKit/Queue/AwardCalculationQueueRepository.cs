using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.AzureKit.Queue
{
	public class AwardCalculationQueueRepository : IAwardCalculationQueueRepository
	{
		private readonly IConfig _config;
		public const string QueueName = "pfawardcalcqueue";

		public AwardCalculationQueueRepository(IConfig config)
		{
			_config = config;
		}

		public async Task Enqueue(AwardCalculationPayload payload)
		{
			var serializedPayload = JsonSerializer.Serialize(payload);
			var client = await GetQueueClient();
			await client.SendMessageAsync(serializedPayload);
		}

#pragma warning disable 1998
		public async Task<KeyValuePair<string, int>> Dequeue()
#pragma warning restore 1998
		{
			throw new System.NotImplementedException($"{nameof(Dequeue)} should never be called because it's automatically bound to an Azure function.");
		}

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
}