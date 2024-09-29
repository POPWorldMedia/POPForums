namespace PopForums.Email;

public interface IEmailWorker
{
	void Execute();
}

public class EmailWorker(ISettingsManager settingsManager, ISmtpWrapper smtpWrapper, IQueuedEmailMessageRepository queuedEmailMessageRepository, IEmailQueueRepository emailQueueRepository, IErrorLog errorLog) : IEmailWorker
{
	public async void Execute()
	{
		try
		{
			var messageGroup = new List<QueuedEmailMessage>();
			for (var i = 1; i <= settingsManager.Current.MailerQuantity; i++)
			{
				var payload = emailQueueRepository.Dequeue().Result;
				if (payload == null)
					break;
				if (payload.EmailQueuePayloadType == EmailQueuePayloadType.DeleteMassMessage)
					throw new NotImplementedException($"EmailQueuePayloadType {payload.EmailQueuePayloadType} not implemented.");
				var queuedMessage = queuedEmailMessageRepository.GetMessage(payload.MessageID).Result;
				if (payload.EmailQueuePayloadType == EmailQueuePayloadType.MassMessage)
				{
					queuedMessage.ToEmail = payload.ToEmail;
					queuedMessage.ToName = payload.ToName;
				}
				if (queuedMessage == null)
					break;
				messageGroup.Add(queuedMessage);
				await queuedEmailMessageRepository.DeleteMessage(queuedMessage.MessageID);
			}
			Parallel.ForEach(messageGroup, message =>
			{
				try
				{
					smtpWrapper.Send(message);
				}
				catch (Exception exc)
				{
					if (message == null)
						errorLog.Log(exc, ErrorSeverity.Email, "There was no message for the MailWorker to send.");
					else
						errorLog.Log(exc, ErrorSeverity.Email, $"MessageID: {message.MessageID}, To: <{message.ToEmail}> {message.ToName}, Subject: {message.Subject}");
				}
			});
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}