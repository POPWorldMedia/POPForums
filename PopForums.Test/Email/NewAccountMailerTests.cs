using System.Net.Mail;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Test.Models;

namespace PopForums.Test.Email
{
	[TestFixture]
	public class NewAccountMailerTests
	{
		[Test]
		public void SendCallsSmtpWrapper()
		{
			var wrapper = new Mock<ISmtpWrapper>();
			var resultMessage = new MailMessage();
			wrapper.Setup(w => w.Send(It.IsAny<MailMessage>()))
				.Returns(SmtpStatusCode.Ok)
				.Callback<MailMessage>(msg => resultMessage = msg);
			const string mailerAddress = "a@b.com";
			const string forumTitle = "superawesome";
			var user = UserTest.GetTestUser();
			var settings = new Settings { MailerAddress = mailerAddress, ForumTitle = forumTitle};
			var settingsManager = new Mock<ISettingsManager>();
			settingsManager.Setup(s => s.Current).Returns(settings);
			var mailer = new NewAccountMailer(settingsManager.Object, wrapper.Object);
			var result = mailer.Send(user, "http://blah/");
			Assert.AreEqual(SmtpStatusCode.Ok, result);
			Assert.AreEqual(resultMessage.To[0].DisplayName, user.Name);
			Assert.AreEqual(resultMessage.To[0].Address, user.Email);
			Assert.AreEqual(resultMessage.From.DisplayName, forumTitle);
			Assert.AreEqual(resultMessage.From.Address, mailerAddress);
		}
	}
}
