using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Email
{
	public interface ISubscribedTopicEmailComposer
	{
		void ComposeAndQueue(Topic topic, User user, string topicLink, string unsubscribeLink);
	}

	public class SubscribedTopicEmailComposer : ISubscribedTopicEmailComposer
	{
		public SubscribedTopicEmailComposer(ISettingsManager settingsManager, IQueuedEmailService queuedEmailService)
		{
			_settingsManager = settingsManager;
			_queuedEmailService = queuedEmailService;

		}

		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailService _queuedEmailService;


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
			_queuedEmailService.CreateAndQueueEmail(message);
		}
	}
}
