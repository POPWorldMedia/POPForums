using System;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Email
{
	public interface IUserEmailer
	{
		Task<bool> IsUserEmailable(User user);
		Task ComposeAndQueue(User toUser, User fromUser, string ip, string subject, string text);
	}

	public class UserEmailer : IUserEmailer
	{
		public UserEmailer(IProfileService profileService, ISettingsManager settingsManager, IQueuedEmailService queuedEmailService)
		{
			_profileService = profileService;
			_settingsManager = settingsManager;
			_queuedEmailService = queuedEmailService;
		}

		private readonly IProfileService _profileService;
		private readonly ISettingsManager _settingsManager;
		private readonly IQueuedEmailService _queuedEmailService;

		public async Task<bool> IsUserEmailable(User user)
		{
			var profile = await _profileService.GetProfile(user);
			return profile.ShowDetails;
		}

		public async Task ComposeAndQueue(User toUser, User fromUser, string ip, string subject, string text)
		{
			if (await IsUserEmailable(toUser) == false)
				throw new Exception("Can't send e-mail to a user who hides their details.");

			if (toUser == null)
				throw new ArgumentNullException("toUser");
			if (fromUser == null)
				throw new ArgumentNullException("fromUser");
			var settings = _settingsManager.Current;
			var body = $@"E-mail sent via {settings.ForumTitle} (senders's IP: {ip}):

{text}

______________________

{settings.MailSignature}";
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
			await _queuedEmailService.CreateAndQueueEmail(message);
		}
	}
}
