using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Email
{
	public interface IForgotPasswordMailer
	{
		void ComposeAndQueue(User user, string resetLink);
	}

	public class ForgotPasswordMailer : IForgotPasswordMailer
	{
		public ForgotPasswordMailer(ISettingsManager settingsManager, IQueuedEmailService queuedEmailService)
		{
			_settingsManager = settingsManager;
			_queuedEmailService = queuedEmailService;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailService _queuedEmailService;

		public void ComposeAndQueue(User user, string resetLink)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (String.IsNullOrEmpty(resetLink))
				throw new ArgumentException("resetLink");
			var settings = _settingsManager.Current;
			var body = String.Format(Resources.ForgotPasswordEmail
				, settings.ForumTitle, resetLink, settings.MailSignature, Environment.NewLine);
			var message = new QueuedEmailMessage
			{
				Body = body,
				Subject = String.Format(Resources.ForgotPasswordSubject, settings.ForumTitle),
				ToEmail = user.Email,
				ToName = user.Name,
				FromEmail = settings.MailerAddress,
				FromName = settings.ForumTitle,
				QueueTime = DateTime.UtcNow
			};
			_queuedEmailService.CreateAndQueueEmail(message);
		}
	}
}