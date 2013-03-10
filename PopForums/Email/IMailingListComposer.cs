using PopForums.Models;

namespace PopForums.Email
{
	public interface IMailingListComposer
	{
		void ComposeAndQueue(User user, string subject, string body, string htmlBody, string unsubscribeLink);
	}
}