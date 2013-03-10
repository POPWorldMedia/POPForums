using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Email
{
	public class UserEmailer : IUserEmailer
	{
		public UserEmailer(IProfileService profileService, ISettingsManager settingsManager, IQueuedEmailMessageRepository queuedEmailRepo)
		{
			_profileService = profileService;
			_settingsManager = settingsManager;
			_queuedQueuedEmailRepo = queuedEmailRepo;
		}

		private readonly IProfileService _profileService;
		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailMessageRepository _queuedQueuedEmailRepo;

		public bool IsUserEmailable(User user)
		{
			var profile = _profileService.GetProfile(user);
			return profile.ShowDetails;
		}

		public void ComposeAndQueue(User toUser, User fromUser, string ip, string subject, string text)
		{
			if (!IsUserEmailable(toUser))
				throw new Exception("Can't send e-mail to a user who hides their details.");

			if (toUser == null)
				throw new ArgumentNullException("toUser");
			if (fromUser == null)
				throw new ArgumentNullException("fromUser");
			var settings = _settingsManager.Current;
			var body = String.Format(@"E-mail sent via {0} (senders's IP: {1}):

{2}

______________________
{3}"
				, settings.ForumTitle, ip, text, settings.MailSignature);
			var message = new QueuedEmailMessage
			{
				Body = body,
				Subject = subject,
				ToEmail = toUser.Email,
				ToName = toUser.Name,
				FromEmail = fromUser.Email,
				FromName = fromUser.Name,
				QueueTime = DateTime.UtcNow
			};
			_queuedQueuedEmailRepo.CreateMessage(message);
		}
	}
}
