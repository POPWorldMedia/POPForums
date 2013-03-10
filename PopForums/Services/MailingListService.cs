using System;
using System.Threading;
using PopForums.Email;
using PopForums.Models;

namespace PopForums.Services
{
	public class MailingListService : IMailingListService
	{
		public MailingListService(IUserService userService, IMailingListComposer mailingListComposer)
		{
			UserService = userService;
			MailingListComposer = mailingListComposer;
		}

		public IUserService UserService { get; private set; }
		public IMailingListComposer MailingListComposer { get; private set; }

		public void MailUsers(string subject, string body, string htmlBody, Func<User, string> unsubscribeLinkGenerator)
		{
			new Thread(() =>
			{
				var users = UserService.GetSubscribedUsers();
				foreach (var user in users)
				{
					var unsubLink = unsubscribeLinkGenerator(user);
					MailingListComposer.ComposeAndQueue(user, subject, body, htmlBody, unsubLink);
				}
			}).Start();
		}
	}
}
