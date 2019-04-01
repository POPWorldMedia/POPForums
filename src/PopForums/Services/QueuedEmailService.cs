using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IQueuedEmailService
	{
		void CreateAndQueueEmail(QueuedEmailMessage queuedEmailMessage);
	}

	public class QueuedEmailService : IQueuedEmailService
	{
		private readonly IQueuedEmailMessageRepository _queuedEmailMessageRepository;
		private readonly IEmailQueueRepository _emailQueueRepository;
		private readonly ITenantService _tenantService;

		public QueuedEmailService(IQueuedEmailMessageRepository queuedEmailMessageRepository, IEmailQueueRepository emailQueueRepository, ITenantService tenantService)
		{
			_queuedEmailMessageRepository = queuedEmailMessageRepository;
			_emailQueueRepository = emailQueueRepository;
			_tenantService = tenantService;
		}

		public void CreateAndQueueEmail(QueuedEmailMessage queuedEmailMessage)
		{
			var id = _queuedEmailMessageRepository.CreateMessage(queuedEmailMessage);
			var tenantID = _tenantService.GetTenant();
			var payload = new EmailQueuePayload { MessageID = id, EmailQueuePayloadType = EmailQueuePayloadType.FullMessage, TenantID = tenantID };
			_emailQueueRepository.Enqueue(payload);
		}
	}
}