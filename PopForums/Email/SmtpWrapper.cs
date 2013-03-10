using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.Email
{
	public class SmtpWrapper : ISmtpWrapper
	{
		public SmtpWrapper(ISettingsManager settingsManager, IErrorLog errorLog)
		{
			_settingsManager = settingsManager;
			_errorLog = errorLog;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IErrorLog _errorLog;

		public SmtpStatusCode Send(QueuedEmailMessage message)
		{
			if (message == null)
				throw new ArgumentNullException("message");
			var from = new MailAddress(message.FromEmail, message.FromName);
			var to = new MailAddress(message.ToEmail, message.ToName);
			var mailMessage = new MailMessage(from, to)
			                  	{
			                  		Subject = message.Subject,
			                  		Body = message.Body
								};
			if (!String.IsNullOrWhiteSpace(message.HtmlBody))
			{
				var altView = AlternateView.CreateAlternateViewFromString(message.HtmlBody, Encoding.UTF8, "text/html");
				altView.TransferEncoding = TransferEncoding.SevenBit;
				mailMessage.AlternateViews.Add(altView);
			}
			mailMessage.Headers.Add("X-MessageID", message.MessageID.ToString());
			return Send(mailMessage);
		}

		public SmtpStatusCode Send(MailMessage message)
		{
			message.Headers.Add("X-Mailer", "POP Forums v9");
			SmtpStatusCode result;
			var settings = _settingsManager.Current;
			var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort) {EnableSsl = settings.UseSslSmtp};
			if (settings.UseEsmtp)
				client.Credentials = new NetworkCredential(settings.SmtpUser, settings.SmtpPassword);
			try
			{
				client.Send(message);
				result = SmtpStatusCode.Ok;
			}
			catch (SmtpException exc)
			{
				result = exc.StatusCode;
				_errorLog.Log(exc, ErrorSeverity.Email, String.Format("To: {0}, Subject: {1}", message.To[0].Address, message.Subject));
			}
			finally
			{
				client.Dispose();
			}
			return result;
		}
	}
}
