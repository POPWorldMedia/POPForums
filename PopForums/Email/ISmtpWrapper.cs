using System.Net.Mail;
using PopForums.Models;

namespace PopForums.Email
{
	public interface ISmtpWrapper
	{
		SmtpStatusCode Send(QueuedEmailMessage message);
		SmtpStatusCode Send(MailMessage message);
	}
}