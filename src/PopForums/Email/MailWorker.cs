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

		public void SendQueuedMessages(ISettingsManager settingsManager, ISmtpWrapper smtpWrapper, IQueuedEmailMessageRepository queuedEmailRepository, IEmailQueueRepository emailQueueRepository, IErrorLog errorLog)
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
					var payload = emailQueueRepository.Dequeue().Result;
					if (payload == null)
						break;
					if (payload.EmailQueuePayloadType == EmailQueuePayloadType.DeleteMassMessage)
						throw new NotImplementedException($"EmailQueuePayloadType {payload.EmailQueuePayloadType} not implemented.");
					var queuedMessage = queuedEmailRepository.GetMessage(payload.MessageID).Result;
					if (payload.EmailQueuePayloadType == EmailQueuePayloadType.MassMessage)
					{
						queuedMessage.ToEmail = payload.ToEmail;
						queuedMessage.ToName = payload.ToName;
					}
					if (queuedMessage == null)
						break;
					messageGroup.Add(queuedMessage);
					if (payload.EmailQueuePayloadType == EmailQueuePayloadType.FullMessage)
						queuedEmailRepository.DeleteMessage(queuedMessage.MessageID);
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