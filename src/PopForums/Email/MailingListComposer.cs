using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Email
{
	public interface IMailingListComposer
	{
		void ComposeAndQueue(User user, string subject, string body, string htmlBody, string unsubscribeLink);
	}

	public class MailingListComposer : IMailingListComposer
	{
		public MailingListComposer(ISettingsManager settingsManager, IQueuedEmailService queuedEmailService)
		{
			_settingsManager = settingsManager;
			_queuedEmailService = queuedEmailService;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailService _queuedEmailService;

		public void ComposeAndQueue(User user, string subject, string body, string htmlBody, string unsubscribeLink)
		{
			var settings = _settingsManager.Current;
			var ps = $"{Environment.NewLine}{Environment.NewLine}Unsubscribe: {unsubscribeLink}";
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
			if (!string.IsNullOrWhiteSpace(htmlBody))
				message.HtmlBody = $"{htmlBody}<p>Unsubscribe: <a href=\"{unsubscribeLink}\">{unsubscribeLink}</a></p>";
			_queuedEmailService.CreateAndQueueEmail(message);
		}
	}
}
