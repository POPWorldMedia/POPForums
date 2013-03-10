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
		private static readonly object _syncRoot = new Object();

		private MailWorker()
		{
			// only allow Instance to create a new instance
		}

		public void SendQueuedMessages(ISettingsManager settingsManager, ISmtpWrapper smtpWrapper, IQueuedEmailMessageRepository queuedEmailRepository, IErrorLog errorLog)
		{
			if (!Monitor.TryEnter(_syncRoot))
			{
				return;
			}
			try
			{
				var messageGroup = new List<QueuedEmailMessage>();
				for (var i = 1; i <= settingsManager.Current.MailerQuantity; i++)
				{
					var queuedMessage = queuedEmailRepository.GetOldestQueuedEmailMessage();
					if (queuedMessage == null)
						break;
					messageGroup.Add(queuedMessage);
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
							errorLog.Log(exc, ErrorSeverity.Email, String.Format("MessageID: {0}, To: <{1}> {2}, Subject: {3}", message.MessageID, message.ToEmail, message.ToName, message.Subject));
					}
				});
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}
			finally
			{
				Monitor.Exit(_syncRoot);
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