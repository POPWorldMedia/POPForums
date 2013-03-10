using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Email
{
	public class ForgotPasswordMailer : IForgotPasswordMailer
	{
		public ForgotPasswordMailer(ISettingsManager settingsManager, IQueuedEmailMessageRepository queuedEmailRepo)
		{
			_settingsManager = settingsManager;
			_queuedQueuedEmailRepo = queuedEmailRepo;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailMessageRepository _queuedQueuedEmailRepo;

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
			_queuedQueuedEmailRepo.CreateMessage(message);
		}
	}
}