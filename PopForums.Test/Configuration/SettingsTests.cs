using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Test.Configuration
{
	[TestFixture]
	public class SettingsTests
	{
		[Test]
		public void LoadDefaults()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;

			Assert.AreEqual(settings.TermsOfService, String.Empty);
			Assert.AreEqual(settings.ServerTimeZone, -5);
			Assert.IsTrue(settings.IsNewUserApproved);
			Assert.AreEqual(settings.TopicsPerPage, 20);
			Assert.AreEqual(settings.PostsPerPage, 20);
			Assert.AreEqual(settings.ForumTitle, String.Empty);
		}

		[Test]
		public void LoadFromRepo()
		{
			const string tos = "blah blah blah";
			const int timeZone = -8;
			const bool isNewUserApproved = false;
			const int topicsPerPage = 72;
			const int postsPerPage = 42;
			const string title = "superawesome forum";
			var dictionary = new Dictionary<string, string>
			                 	{
			                 		{"TermsOfService", tos},
									{"ServerTimeZone", timeZone.ToString()},
									{"IsNewUserApproved", isNewUserApproved.ToString()},
									{"TopicsPerPage", topicsPerPage.ToString()},
									{"PostsPerPage", postsPerPage.ToString()},
									{"ForumTitle", title}
			                 	};
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(dictionary);
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;

			Assert.AreEqual(settings.TermsOfService, tos);
			Assert.AreEqual(settings.ServerTimeZone, timeZone);
			Assert.IsFalse(settings.IsNewUserApproved);
			Assert.AreEqual(settings.TopicsPerPage, topicsPerPage);
			Assert.AreEqual(settings.PostsPerPage, postsPerPage);
			Assert.AreEqual(settings.ForumTitle, title);
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Test]
		public void LoadFromRepoThenCache()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;
			var settings2 = settingsManager.Current;
			var settings3 = settingsManager.Current;

			Assert.AreEqual(settings, settings2);
			Assert.AreEqual(settings, settings3);
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Test]
		public void LoadFromRepoWhenStale()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			settingsRepo.Setup(s => s.IsStale(It.IsAny<DateTime>())).Returns(true);
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;
			settings = settingsManager.Current;
			settingsRepo.Verify(s => s.Get(), Times.Exactly(2));
		}

		[Test]
		public void DoNotLoadFromRepoWhenNotStale()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			settingsRepo.Setup(s => s.IsStale(It.IsAny<DateTime>())).Returns(false);
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;
			settings = settingsManager.Current;
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Test]
		public void SaveCurrent()
		{
			const string tos = "blah blah blah";
			const int timeZone = -8;
			const bool isNewUserApproved = false;
			const int topicsPerPage = 72;
			const int postsPerPage = 42;
			const int minimumSecondsBetweenPosts = 33;
			const string title = "superawesome forum";
			const string smtpServer = "mail.?.com";
			const int smtpPort = 69;
			const string mailerAddress = "a@b.com";
			const bool useEsmtp = true;
			const string smtpUser = "b@c.com";
			const string smtpPassword = "jkl";
			const int mailSendingInverval = 500;
			const bool useSslSmtp = true;
			const bool serverDaylightSaving = false;
			const int sessionLength = 20;
			const string censorWords = "shit";
			const string censorCharacter = "x";
			const bool allowImages = true;
			const bool logSecurity = false;
			const bool logModeration = false;
			const bool logErrors = false;
			const bool isNewUserImageApproved = true;
			const int searchIndexingInterval = 768;
			const bool isSearchIndexingEnabled = false;
			const bool isMailerEnabled = false;
			const int userImageMaxHeight = 999;
			const int userImageMaxWidth = 888;
			const int userImageMaxkBytes = 77;
			const int userAvatarMaxHeight = 665;
			const int userAvatarMaxWidth = 554;
			const int userAvatarMaxkBytes = 33;
			const string mailSignature = "this is the sig";
			const int awardCalcInterval = 5230;
			const int mailerQuantity = 914;
			var dictionary = new Dictionary<string, object>
			                 	{
			                 		{"TermsOfService", tos},
									{"ServerTimeZone", timeZone},
									{"ServerDaylightSaving", serverDaylightSaving},
									{"IsNewUserApproved", isNewUserApproved},
									{"TopicsPerPage", topicsPerPage},
									{"PostsPerPage", postsPerPage},
									{"ForumTitle", title},
									{"MinimumSecondsBetweenPosts", minimumSecondsBetweenPosts},
									{"SmtpServer", smtpServer},
									{"SmtpPort", smtpPort},
									{"MailerAddress", mailerAddress},
									{"UseEsmtp", useEsmtp},
									{"SmtpUser", smtpUser},
									{"SmtpPassword", smtpPassword},
									{"MailSendingInverval", mailSendingInverval},
									{"UseSslSmtp", useSslSmtp},
									{"SessionLength", sessionLength},
									{"CensorWords", censorWords},
									{"CensorCharacter", censorCharacter},
									{"AllowImages", allowImages},
									{"LogSecurity", logSecurity},
									{"LogModeration", logModeration},
									{"LogErrors", logErrors},
									{"IsNewUserImageApproved", isNewUserImageApproved},
									{"SearchIndexingInterval", searchIndexingInterval},
									{"IsSearchIndexingEnabled", isSearchIndexingEnabled},
									{"IsMailerEnabled", isMailerEnabled},
									{"UserImageMaxHeight", userImageMaxHeight},
									{"UserImageMaxWidth", userImageMaxWidth},
									{"UserImageMaxkBytes", userImageMaxkBytes},
									{"UserAvatarMaxHeight", userAvatarMaxHeight},
									{"UserAvatarMaxWidth", userAvatarMaxWidth},
									{"UserAvatarMaxkBytes", userAvatarMaxkBytes},
									{"MailSignature", mailSignature},
									{"ScoringGameCalculatorInterval", awardCalcInterval},
									{"MailerQuantity", mailerQuantity}
			                 	};

			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);
			settingsManager.FlushCurrent();

			var settings = settingsManager.Current;
			settings.TermsOfService = tos;
			settings.ServerTimeZone = timeZone;
			settings.IsNewUserApproved = isNewUserApproved;
			settings.TopicsPerPage = topicsPerPage;
			settings.PostsPerPage = postsPerPage;
			settings.ForumTitle = title;
			settings.MinimumSecondsBetweenPosts = minimumSecondsBetweenPosts;
			settings.SmtpServer = smtpServer;
			settings.SmtpPort = smtpPort;
			settings.MailerAddress = mailerAddress;
			settings.UseEsmtp = useEsmtp;
			settings.SmtpUser = smtpUser;
			settings.SmtpPassword = smtpPassword;
			settings.MailSendingInverval = mailSendingInverval;
			settings.UseSslSmtp = useSslSmtp;
			settings.ServerDaylightSaving = serverDaylightSaving;
			settings.SessionLength = sessionLength;
			settings.CensorWords = censorWords;
			settings.CensorCharacter = censorCharacter;
			settings.AllowImages = allowImages;
			settings.LogSecurity = logSecurity;
			settings.LogModeration = logModeration;
			settings.LogErrors = logErrors;
			settings.IsNewUserImageApproved = isNewUserImageApproved;
			settings.SearchIndexingInterval = searchIndexingInterval;
			settings.IsSearchIndexingEnabled = isSearchIndexingEnabled;
			settings.IsMailerEnabled = isMailerEnabled;
			settings.UserImageMaxHeight = userImageMaxHeight;
			settings.UserImageMaxWidth = userImageMaxWidth;
			settings.UserImageMaxkBytes = userImageMaxkBytes;
			settings.UserAvatarMaxHeight = userAvatarMaxHeight;
			settings.UserAvatarMaxWidth = userAvatarMaxWidth;
			settings.UserAvatarMaxkBytes = userAvatarMaxkBytes;
			settings.MailSignature = mailSignature;
			settings.ScoringGameCalculatorInterval = awardCalcInterval;
			settings.MailerQuantity = mailerQuantity;
			settingsManager.SaveCurrent();

			settingsRepo.Verify(s => s.Save(dictionary), Times.Once());
		}

		[Test]
		public void SaveCurrentFromDictionary()
		{
			const string tos = "blah blah blah";
			const int timeZone = -8;
			const bool isNewUserApproved = false;
			const int topicsPerPage = 72;
			const int postsPerPage = 42;
			const int minimumSecondsBetweenPosts = 33;
			const string title = "superawesome forum";
			const string smtpServer = "mail.?.com";
			const int smtpPort = 69;
			const string mailerAddress = "a@b.com";
			const bool useEsmtp = true;
			const string smtpUser = "b@c.com";
			const string smtpPassword = "jkl";
			const int mailSendingInverval = 500;
			const bool useSslSmtp = true;
			const bool serverDaylightSaving = false;
			const int sessionLength = 20;
			const string censorWords = "shit";
			const string censorCharacter = "x";
			const bool allowImages = true;
			const bool logSecurity = false;
			const bool logModeration = false;
			const bool logErrors = false;
			const bool isNewUserImageApproved = true;
			const int searchIndexingInterval = 768;
			const bool isSearchIndexingEnabled = false;
			const bool isMailerEnabled = false;
			const int userImageMaxHeight = 999;
			const int userImageMaxWidth = 888;
			const int userImageMaxkBytes = 77;
			const int userAvatarMaxHeight = 665;
			const int userAvatarMaxWidth = 554;
			const int userAvatarMaxkBytes = 33;
			const string mailSignature = "this is the sig";
			var dictionary = new Dictionary<string, object>
			                 	{
			                 		{"TermsOfService", tos},
									{"ServerTimeZone", timeZone},
									{"ServerDaylightSaving", serverDaylightSaving},
									{"IsNewUserApproved", isNewUserApproved},
									{"TopicsPerPage", topicsPerPage},
									{"PostsPerPage", postsPerPage},
									{"ForumTitle", title},
									{"MinimumSecondsBetweenPosts", minimumSecondsBetweenPosts},
									{"SmtpServer", smtpServer},
									{"SmtpPort", smtpPort},
									{"MailerAddress", mailerAddress},
									{"UseEsmtp", useEsmtp},
									{"SmtpUser", smtpUser},
									{"SmtpPassword", smtpPassword},
									{"MailSendingInverval", mailSendingInverval},
									{"UseSslSmtp", useSslSmtp},
									{"SessionLength", sessionLength},
									{"CensorWords", censorWords},
									{"CensorCharacter", censorCharacter},
									{"AllowImages", allowImages},
									{"LogSecurity", logSecurity},
									{"LogModeration", logModeration},
									{"LogErrors", logErrors},
									{"IsNewUserImageApproved", isNewUserImageApproved},
									{"SearchIndexingInterval", searchIndexingInterval},
									{"IsSearchIndexingEnabled", isSearchIndexingEnabled},
									{"IsMailerEnabled", isMailerEnabled},
									{"UserImageMaxHeight", userImageMaxHeight},
									{"UserImageMaxWidth", userImageMaxWidth},
									{"UserImageMaxkBytes", userImageMaxkBytes},
									{"UserAvatarMaxHeight", userAvatarMaxHeight},
									{"UserAvatarMaxWidth", userAvatarMaxWidth},
									{"UserAvatarMaxkBytes", userAvatarMaxkBytes},
									{"MailSignature", mailSignature}
			                 	};

			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

			settingsManager.SaveCurrent(dictionary);
			Assert.AreEqual(tos, settingsManager.Current.TermsOfService);
			Assert.AreEqual(timeZone, settingsManager.Current.ServerTimeZone);
			Assert.AreEqual(isNewUserApproved, settingsManager.Current.IsNewUserApproved);
			Assert.AreEqual(topicsPerPage, settingsManager.Current.TopicsPerPage);
			Assert.AreEqual(postsPerPage, settingsManager.Current.PostsPerPage);

			settingsRepo.Verify(s => s.Save(It.IsAny<Dictionary<string, object>>()), Times.Once());
		}
	}
}
