using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Email
{
	public class SubscribedTopicEmailComposer : ISubscribedTopicEmailComposer
	{
		public SubscribedTopicEmailComposer(ISettingsManager settingsManager, IQueuedEmailMessageRepository queuedEmailRepo)
		{
			_settingsManager = settingsManager;
			_queuedQueuedEmailRepo = queuedEmailRepo;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailMessageRepository _queuedQueuedEmailRepo;

		public void ComposeAndQueue(Topic topic, User user, string topicLink, string unsubscribeLink)
		{
			var settings = _settingsManager.Current;
			var body = String.Format(Resources.SubscribedEmailBody, settings.ForumTitle, topic.Title, topicLink, unsubscribeLink, settings.MailSignature, Environment.NewLine);
			var message = new QueuedEmailMessage
			              	{
			              		Body = body, 
								Subject =String.Format(Resources.SubscribedEmailSubject, topic.Title), 
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
