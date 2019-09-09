using System;
using System.Collections.Generic;
using Moq;
using PopForums.Configuration;
using PopForums.Repositories;
using Xunit;

namespace PopForums.Test.Configuration
{
	public class SettingsTests
	{
		[Fact]
		public void LoadDefaults()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

			var settings = settingsManager.Current;

			Assert.Equal(settings.TermsOfService, String.Empty);
			Assert.Equal(settings.ServerTimeZone, -5);
			Assert.True(settings.IsNewUserApproved);
			Assert.Equal(20, settings.TopicsPerPage);
			Assert.Equal(20, settings.PostsPerPage);
			Assert.Equal(settings.ForumTitle, String.Empty);
		}

		[Fact]
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

			var settings = settingsManager.Current;

			Assert.Equal(settings.TermsOfService, tos);
			Assert.Equal(settings.ServerTimeZone, timeZone);
			Assert.False(settings.IsNewUserApproved);
			Assert.Equal(settings.TopicsPerPage, topicsPerPage);
			Assert.Equal(settings.PostsPerPage, postsPerPage);
			Assert.Equal(settings.ForumTitle, title);
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Fact]
		public void LoadFromRepoThenCache()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

			var settings = settingsManager.Current;
			var settings2 = settingsManager.Current;
			var settings3 = settingsManager.Current;

			Assert.Equal(settings, settings2);
			Assert.Equal(settings, settings3);
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Fact]
		public void LoadFromRepoWhenStale()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			settingsRepo.Setup(s => s.IsStale(It.IsAny<DateTime>())).Returns(true);
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

			var settings = settingsManager.Current;
			settings = settingsManager.Current;
			settingsRepo.Verify(s => s.Get(), Times.Exactly(2));
		}

		[Fact]
		public void DoNotLoadFromRepoWhenNotStale()
		{
			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			settingsRepo.Setup(s => s.IsStale(It.IsAny<DateTime>())).Returns(false);
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

			var settings = settingsManager.Current;
			settings = settingsManager.Current;
			settingsRepo.Verify(s => s.Get(), Times.Once());
		}

		[Fact]
		public void SaveCurrent()
		{
			const string tos = "blah blah blah";
			const int timeZone = -8;
			const bool serverDaylightSaving = false;
			const bool isNewUserApproved = false;
			const int topicsPerPage = 72;
			const int postsPerPage = 42;
			const string title = "superawesome forum";
			const int minimumSecondsBetweenPosts = 33;
			const string smtpServer = "mail.?.com";
			const int smtpPort = 69;
			const string mailerAddress = "a@b.com";
			const bool useEsmtp = true;
			const string smtpUser = "b@c.com";
			const string smtpPassword = "jkl";
			const int mailSendingInverval = 500;
			const bool useSslSmtp = true;
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
			const bool useGoogleLogin = true;
			const bool useFacebookLogin = true;
			const string facebookAppID = "oiwoeighw";
			const string facebookAppSecret = "oiwhwohgcgr";
			const bool useMicrosoftLogin = true;
			const string microsoftClientID = "hhvcwefwege";
			const string microsoftClientSecret = "oiwhgoigrccaa";
			const int youTubeHeight = 360;
			const int youTubeWidth = 640;
			const string googleClientId = "ohigfewgf";
			const string googleClientSecret = "y0yt0w4gweg";
			const bool useOAuth2Login = true;
			const string oAuth2ClientID = "efew";
			const string oAuth2ClientSecret = "cons";
			const string oAuth2DisplayName = "we3t";
			const string oAuth2LoginUrl = "ef";
			const string oAuth2TokenUrl = "w";
			const string oAuth2Scope = "email";
			const bool isClosingAgedTopics = true;
			const int closeAgedTopicsDays = 757;
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
									{"MailerQuantity", mailerQuantity},
									{"UseGoogleLogin", useGoogleLogin},
									{"UseFacebookLogin", useFacebookLogin},
									{"FacebookAppID", facebookAppID},
									{"FacebookAppSecret", facebookAppSecret},
									{"UseMicrosoftLogin", useMicrosoftLogin},
									{"MicrosoftClientID", microsoftClientID},
									{"MicrosoftClientSecret", microsoftClientSecret},
									{"YouTubeHeight", youTubeHeight},
									{"YouTubeWidth", youTubeWidth},
									{"GoogleClientId", googleClientId},
									{"GoogleClientSecret", googleClientSecret},
									{"UseOAuth2Login", useOAuth2Login},
									{"OAuth2ClientID", oAuth2ClientID },
									{"OAuth2ClientSecret", oAuth2ClientSecret },
									{"OAuth2LoginUrl", oAuth2LoginUrl },
									{"OAuth2TokenUrl", oAuth2TokenUrl },
									{"OAuth2DisplayName", oAuth2DisplayName },
									{"OAuth2Scope", oAuth2Scope },
									{"IsClosingAgedTopics", isClosingAgedTopics},
									{"CloseAgedTopicsDays", closeAgedTopicsDays}
								 };

			var settingsRepo = new Mock<ISettingsRepository>();
			settingsRepo.Setup(s => s.Get()).Returns(new Dictionary<string, string>());
			var errorLog = new Mock<IErrorLog>();
			var settingsManager = new SettingsManager(settingsRepo.Object, errorLog.Object);

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
			settings.UseGoogleLogin = useGoogleLogin;
			settings.UseFacebookLogin = useFacebookLogin;
			settings.FacebookAppID = facebookAppID;
			settings.FacebookAppSecret = facebookAppSecret;
			settings.UseMicrosoftLogin = useMicrosoftLogin;
			settings.MicrosoftClientID = microsoftClientID;
			settings.MicrosoftClientSecret = microsoftClientSecret;
			settings.YouTubeHeight = youTubeHeight;
			settings.YouTubeWidth = youTubeWidth;
			settings.GoogleClientId = googleClientId;
			settings.GoogleClientSecret = googleClientSecret;
			settings.UseOAuth2Login = useOAuth2Login;
			settings.OAuth2ClientID = oAuth2ClientID;
			settings.OAuth2ClientSecret = oAuth2ClientSecret;
			settings.OAuth2LoginUrl = oAuth2LoginUrl;
			settings.OAuth2TokenUrl = oAuth2TokenUrl;
			settings.OAuth2DisplayName = oAuth2DisplayName;
			settings.OAuth2Scope = oAuth2Scope;
			settings.IsClosingAgedTopics = isClosingAgedTopics;
			settings.CloseAgedTopicsDays = closeAgedTopicsDays;
			settingsManager.SaveCurrent();

			settingsRepo.Verify(s => s.Save(dictionary), Times.Once());
		}
	}
}
