using NSubstitute.Extensions;
using NSubstitute.ReturnsExtensions;

namespace PopForums.Test.Email;

public class EmailWorkerTests
{
	private ISettingsManager _settingsManager;
	private ISmtpWrapper _smtpWrapper;
	private IQueuedEmailMessageRepository _queuedEmailMessageRepository;
	private IEmailQueueRepository _emailQueueRepository;
	private IErrorLog _errorLog;

	private IEmailWorker GetWorker()
	{
		_settingsManager = Substitute.For<ISettingsManager>();
		_smtpWrapper = Substitute.For<ISmtpWrapper>();
		_queuedEmailMessageRepository = Substitute.For<IQueuedEmailMessageRepository>();
		_emailQueueRepository = Substitute.For<IEmailQueueRepository>();
		_errorLog = Substitute.For<IErrorLog>();
		return new EmailWorker(_settingsManager, _smtpWrapper, _queuedEmailMessageRepository, _emailQueueRepository,
			_errorLog);
	}
	
	[Fact]
	public void DoNothingWhenNoPayload()
	{
		var worker = GetWorker();
		_emailQueueRepository.Dequeue().ReturnsNull();
		
		worker.Execute();

		_queuedEmailMessageRepository.DidNotReceiveWithAnyArgs().GetMessage(Arg.Any<int>());
		_queuedEmailMessageRepository.DidNotReceiveWithAnyArgs().DeleteMessage(Arg.Any<int>());
		_smtpWrapper.DidNotReceiveWithAnyArgs().Send(Arg.Any<QueuedEmailMessage>());
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}
	
	[Fact]
	public async Task LogExceptionWhenEmailQueuePayloadTypeIsDeleteMassMessage()
	{
		var worker = GetWorker();
		_settingsManager.Current.MailerQuantity.Returns(4);
		_emailQueueRepository.Dequeue().Returns(new EmailQueuePayload { EmailQueuePayloadType = EmailQueuePayloadType.DeleteMassMessage });
		
		await worker.Execute();

		_errorLog.Received(1).Log(Arg.Any<NotImplementedException>(), ErrorSeverity.Error);
	}

    [Fact]
    public async Task ProcessAMessage()
    {
        var worker = GetWorker();
        _settingsManager.Current.MailerQuantity.Returns(4);
        var payload = new EmailQueuePayload { EmailQueuePayloadType = EmailQueuePayloadType.MassMessage, MessageID = 1, ToEmail = "test@example.com", ToName = "Test User" };
        var queuedMessage = new QueuedEmailMessage { MessageID = 1 };
        _emailQueueRepository.Dequeue().Returns(payload, null, null);
        _queuedEmailMessageRepository.GetMessage(1).Returns(queuedMessage);

        await worker.Execute();

        Assert.Equal("test@example.com", queuedMessage.ToEmail);
        Assert.Equal("Test User", queuedMessage.ToName);
        await _queuedEmailMessageRepository.Received(1).DeleteMessage(1);
        _smtpWrapper.Received(1).Send(queuedMessage);
    }

    [Fact]
    public async Task DoNothingWhenQueuedMessageIsNull()
    {
        var worker = GetWorker();
        _settingsManager.Current.MailerQuantity.Returns(4);
        var payload = new EmailQueuePayload { EmailQueuePayloadType = EmailQueuePayloadType.MassMessage, MessageID = 1 };
        _emailQueueRepository.Dequeue().Returns(payload);
        _queuedEmailMessageRepository.GetMessage(1).ReturnsNull();

        await worker.Execute();

        await _queuedEmailMessageRepository.DidNotReceiveWithAnyArgs().DeleteMessage(Arg.Any<int>());
        _smtpWrapper.DidNotReceiveWithAnyArgs().Send(Arg.Any<QueuedEmailMessage>());
    }

    [Fact]
    public async Task LogExceptionWhenSmtpSendFails()
    {
        var worker = GetWorker();
        _settingsManager.Current.MailerQuantity.Returns(4);
        var payload = new EmailQueuePayload { EmailQueuePayloadType = EmailQueuePayloadType.DeleteMassMessage, MessageID = 1 };
        var queuedMessage = new QueuedEmailMessage { MessageID = 1 };
        _emailQueueRepository.Dequeue().Returns(payload);
        _queuedEmailMessageRepository.GetMessage(1).Returns(queuedMessage);
        _smtpWrapper.When(x => x.Send(queuedMessage)).Do(_ => throw new Exception());

        await worker.Execute();

        _errorLog.Received(1).Log(Arg.Any<Exception>(), ErrorSeverity.Error);
    }
}