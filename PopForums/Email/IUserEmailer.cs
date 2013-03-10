using PopForums.Models;

namespace PopForums.Email
{
	public interface IUserEmailer
	{
		bool IsUserEmailable(User user);
		void ComposeAndQueue(User toUser, User fromUser, string ip, string subject, string text);
	}
}