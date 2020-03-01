using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Repositories;

namespace PopForums.AzureKit.Queue
{
	public class EmailQueueRepository : IEmailQueueRepository
	{
		private readonly IConfig _config;
		public const string QueueName = "pfemailqueue";

		public EmailQueueRepository(IConfig config)
		{
			_config = config;
		}

		public async Task Enqueue(EmailQueuePayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			var message = new CloudQueueMessage(serializedPayload);
			var queue = await GetQueue();
			await queue.AddMessageAsync(message);
		}

		public Task<EmailQueuePayload> Dequeue()
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