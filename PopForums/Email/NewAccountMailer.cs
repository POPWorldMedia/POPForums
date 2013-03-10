using System;
using System.Net.Mail;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.Email
{
	public class NewAccountMailer : INewAccountMailer
	{
		public NewAccountMailer(ISettingsManager settingsManager, ISmtpWrapper smtpWrapper)
		{
			_settingsManager = settingsManager;
			_smtpWrapper = smtpWrapper;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly ISmtpWrapper _smtpWrapper;

		public SmtpStatusCode Send(User user, string verifyUrl)
		{
			var settings = _settingsManager.Current;
			if (String.IsNullOrWhiteSpace(settings.MailerAddress))
				throw new Exception("There is no MailerAddress to send e-mail from. Perhaps you didn't set up the settings.");
			var message = new MailMessage(
				new MailAddress(settings.MailerAddress, settings.ForumTitle), 
				new MailAddress(user.Email, user.Name));
			message.Subject = String.Format(Resources.RegisterEmailSubject, settings.ForumTitle);
			string body;
			if (settings.IsNewUserApproved)
				body = String.Format(NewUserApprovedEmail, settings.ForumTitle, settings.MailSignature, Environment.NewLine);
			else
				body = String.Format(NewUserVerifyEmail, settings.ForumTitle, verifyUrl + "/" + user.AuthorizationKey, verifyUrl, user.AuthorizationKey, settings.MailSignature, Environment.NewLine);
			message.Body = body;
			return _smtpWrapper.Send(message);
		}

		public virtual string NewUserApprovedEmail
		{
			get { return Resources.RegisterEmailThankYou; }
		}

		public virtual string NewUserVerifyEmail
		{
			get { return Resources.RegisterEmailThankYou; }
		}
	}
}
