using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Email
{
	public class MailWorker
	{
		private static readonly object SyncRoot = new object();

		private MailWorker()
		{
			// only allow Instance to create a new instance
		}

		public async Task SendQueuedMessages(ISettingsManager settingsManager, ISmtpWrapper smtpWrapper, IQueuedEmailMessageRepository queuedEmailRepository, IEmailQueueRepository emailQueueRepository, IErrorLog errorLog)
		{
			if (!Monitor.TryEnter(SyncRoot))
			{
				return;
			}
			try
			{
				var messageGroup = new List<QueuedEmailMessage>();
				for (var i = 1; i <= settingsManager.Current.MailerQuantity; i++)
				{
					var payload = await emailQueueRepository.Dequeue();
					if (payload == null)
						break;
					if (payload.EmailQueuePayloadType != EmailQueuePayloadType.FullMessage)
						throw new NotImplementedException($"EmailQueuePayloadType {payload.EmailQueuePayloadType} not implemented.");
					var queuedMessage = await queuedEmailRepository.GetMessage(payload.MessageID);
					if (queuedMessage == null)
						break;
					messageGroup.Add(queuedMessage);
					await queuedEmailRepository.DeleteMessage(queuedMessage.MessageID);
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
			finally
			{
				Monitor.Exit(SyncRoot);
			}
		}

		private static readonly MailWorker _instance = new MailWorker();
		public static MailWorker Instance
		{
			get
			{
				return _instance;
			}
		}
	}
}