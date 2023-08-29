namespace PopForums.Test.Email;

public class NewAccountMailerTests
{
	[Fact]
	public void SendCallsSmtpWrapper()
	{
		var wrapper = Substitute.For<ISmtpWrapper>();
		var resultMessage = new EmailMessage();
		wrapper.Send(Arg.Do<EmailMessage>(msg => resultMessage = msg)).Returns(SmtpStatusCode.Ok);
		const string mailerAddress = "a@b.com";
		const string forumTitle = "superawesome";
		var user = UserTest.GetTestUser();
		var settings = new Settings { MailerAddress = mailerAddress, ForumTitle = forumTitle};
		var settingsManager = Substitute.For<ISettingsManager>();
		settingsManager.Current.Returns(settings);
		var mailer = new NewAccountMailer(settingsManager, wrapper);

		var result = mailer.Send(user, "http://blah/");

		Assert.Equal(SmtpStatusCode.Ok, result);
		Assert.Equal(resultMessage.ToName, user.Name);
		Assert.Equal(resultMessage.ToEmail, user.Email);
		Assert.Equal(forumTitle, resultMessage.FromName);
	}
}