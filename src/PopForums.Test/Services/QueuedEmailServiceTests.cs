using System.Threading.Tasks;
using Moq;
using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class QueuedEmailServiceTests
	{
		private Mock<IQueuedEmailMessageRepository> _queuedEmailMessageRepo;
		private Mock<IEmailQueueRepository> _emailQueueRepo;
		private Mock<ITenantService> _tenantService;

		private QueuedEmailService GetService()
		{
			_queuedEmailMessageRepo = new Mock<IQueuedEmailMessageRepository>();
			_emailQueueRepo = new Mock<IEmailQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			return new QueuedEmailService(_queuedEmailMessageRepo.Object, _emailQueueRepo.Object, _tenantService.Object);
		}

		[Fact]
		public async Task CreateAndQueueEmailCallsRepoWithMessage()
		{
			var service = GetService();
			var message = new QueuedEmailMessage();
			_queuedEmailMessageRepo.Setup(x => x.CreateMessage(message)).ReturnsAsync(1);
			_tenantService.Setup(x => x.GetTenant()).Returns("");

			await service.CreateAndQueueEmail(message);

			_queuedEmailMessageRepo.Verify(x => x.CreateMessage(message), Times.Once);
		}

		[Fact]
		public async Task CreateAndQueueEmailCallsEmailQueueWithCorrectPayload()
		{
			var service = GetService();
			var messageID = 123;
			var message = new QueuedEmailMessage();
			_queuedEmailMessageRepo.Setup(x => x.CreateMessage(message)).ReturnsAsync(messageID);
			var tenantID = "t1";
			_tenantService.Setup(x => x.GetTenant()).Returns(tenantID);
			var payload = new EmailQueuePayload();
			_emailQueueRepo.Setup(x => x.Enqueue(It.IsAny<EmailQueuePayload>())).Callback<EmailQueuePayload>(p => payload = p);

			await service.CreateAndQueueEmail(message);

			Assert.Equal(messageID, payload.MessageID);
			Assert.Equal(EmailQueuePayloadType.FullMessage, payload.EmailQueuePayloadType);
			Assert.Equal(tenantID, payload.TenantID);
		}
	}
}