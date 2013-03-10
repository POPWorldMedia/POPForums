using System;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IMailingListService
	{
		void MailUsers(string subject, string body, string htmlBody, Func<User, string> unsubscribeLinkGenerator);
	}
}