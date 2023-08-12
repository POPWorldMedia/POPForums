namespace PopForums.Test.Services;

public class QueuedEmailServiceTests
{
	private IQueuedEmailMessageRepository _queuedEmailMessageRepo;
	private IEmailQueueRepository _emailQueueRepo;
	private ITenantService _tenantService;

	private QueuedEmailService GetService()
	{
		_queuedEmailMessageRepo = Substitute.For<IQueuedEmailMessageRepository>();
		_emailQueueRepo = Substitute.For<IEmailQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		return new QueuedEmailService(_queuedEmailMessageRepo, _emailQueueRepo, _tenantService);
	}

	[Fact]
	public async Task CreateAndQueueEmailCallsRepoWithMessage()
	{
		var service = GetService();
		var message = new QueuedEmailMessage();
		_queuedEmailMessageRepo.CreateMessage(message).Returns(Task.FromResult(1));
		_tenantService.GetTenant().Returns("");

		await service.CreateAndQueueEmail(message);

		await _queuedEmailMessageRepo.Received().CreateMessage(message);
	}

	[Fact]
	public async Task CreateAndQueueEmailCallsEmailQueueWithCorrectPayload()
	{
		var service = GetService();
		var messageID = 123;
		var message = new QueuedEmailMessage();
		_queuedEmailMessageRepo.CreateMessage(message).Returns(Task.FromResult(messageID));
		var tenantID = "t1";
		_tenantService.GetTenant().Returns(tenantID);
		var payload = new EmailQueuePayload();
		await _emailQueueRepo.Enqueue(Arg.Do<EmailQueuePayload>(x => payload = x));

		await service.CreateAndQueueEmail(message);

		Assert.Equal(messageID, payload.MessageID);
		Assert.Equal(EmailQueuePayloadType.FullMessage, payload.EmailQueuePayloadType);
		Assert.Equal(tenantID, payload.TenantID);
	}
}