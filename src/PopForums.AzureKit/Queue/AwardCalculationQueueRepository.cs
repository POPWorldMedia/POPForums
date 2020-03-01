using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
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
			var serializedPayload = JsonConvert.SerializeObject(payload);
			var message = new CloudQueueMessage(serializedPayload);
			var queue = await GetQueue();
			await queue.AddMessageAsync(message);
		}

#pragma warning disable 1998
		public async Task<KeyValuePair<string, int>> Dequeue()
#pragma warning restore 1998
		{
			throw new System.NotImplementedException($"{nameof(Dequeue)} should never be called because it's automatically bound to an Azure function.");
		}

		private async Task<CloudQueue> GetQueue()
		{
			var storageAccount = CloudStorageAccount.Parse(_config.QueueConnectionString);
			var client = storageAccount.CreateCloudQueueClient();
			var queue = client.GetQueueReference(QueueName);
			await queue.CreateIfNotExistsAsync();
			return queue;
		}
	}
}