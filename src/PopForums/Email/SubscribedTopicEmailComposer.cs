using System;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Email
{
	public interface ISubscribedTopicEmailComposer
	{
		Task ComposeAndQueue(Topic topic, User user, string topicLink, string unsubscribeLink);
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


		public async Task ComposeAndQueue(Topic topic, User user, string topicLink, string unsubscribeLink)
		{
			var settings = _settingsManager.Current;
			var body = string.Format(Resources.SubscribedEmailBody, settings.ForumTitle, topic.Title, topicLink, unsubscribeLink, settings.MailSignature, Environment.NewLine);
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
			await _queuedEmailService.CreateAndQueueEmail(message);
		}
	}
}
