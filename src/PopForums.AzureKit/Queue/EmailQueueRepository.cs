using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
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
			var serializedPayload = JsonSerializer.Serialize(payload);
			var client = await GetQueueClient();
			await client.SendMessageAsync(serializedPayload);
		}

		public Task<EmailQueuePayload> Dequeue()
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