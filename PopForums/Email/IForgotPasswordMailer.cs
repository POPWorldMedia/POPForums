using PopForums.Models;

namespace PopForums.Email
{
	public interface IForgotPasswordMailer
	{
		void ComposeAndQueue(User user, string resetLink);
	}
}