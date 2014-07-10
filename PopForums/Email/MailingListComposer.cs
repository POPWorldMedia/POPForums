using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Email
{
	public class MailingListComposer : IMailingListComposer
	{
		public MailingListComposer(ISettingsManager settingsManager, IQueuedEmailMessageRepository queuedEmailRepo)
		{
			_settingsManager = settingsManager;
			_queuedQueuedEmailRepo = queuedEmailRepo;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailMessageRepository _queuedQueuedEmailRepo;

		public void ComposeAndQueue(User user, string subject, string body, string htmlBody, string unsubscribeLink)
		{
			var settings = _settingsManager.Current;
			var ps = String.Format("{1}{1}Unsubscribe: {0}", unsubscribeLink, Environment.NewLine);
			var message = new QueuedEmailMessage
			{
				Body = body + ps,
				Subject = subject,
				ToEmail = user.Email,
				ToName = user.Name,
				FromEmail = settings.MailerAddress,
				FromName = settings.ForumTitle,
				QueueTime = DateTime.UtcNow
			};
			if (!String.IsNullOrWhiteSpace(htmlBody))
				message.HtmlBody = String.Format("{0}<p>Unsubscribe: <a href=\"{1}\">{1}</a></p>", htmlBody, unsubscribeLink);
			_queuedQueuedEmailRepo.CreateMessage(message);
		}
	}
}
