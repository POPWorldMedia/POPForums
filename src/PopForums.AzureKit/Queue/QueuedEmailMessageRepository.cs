using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Services;
using PopForums.Sql;

namespace PopForums.AzureKit.Queue
{
	public class QueuedEmailMessageRepository : PopForums.Sql.Repositories.QueuedEmailMessageRepository
	{
		private readonly IConfig _config;
		private const string QueueName = "pfemailqueue";

		public QueuedEmailMessageRepository(ISqlObjectFactory sqlObjectFactory, ITenantService tenantService, IConfig config) : base(sqlObjectFactory, tenantService)
		{
			_config = config;
		}

		protected override void WriteMessageToEmailQueue(EmailQueuePayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			var message = new CloudQueueMessage(serializedPayload);
			var queue = GetQueue().Result;
			queue.AddMessageAsync(message);
		}

		protected override EmailQueuePayload DequeueEmailQueuePayload()
		{
			return null;
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