namespace PopForums.Test.Configuration;

public class SettingsTests
{
	[Fact]
	public void LoadDefaults()
	{
		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(new Dictionary<string, string>());
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;

		Assert.Equal(settings.TermsOfService, String.Empty);
		Assert.True(settings.IsNewUserApproved);
		Assert.Equal(20, settings.TopicsPerPage);
		Assert.Equal(20, settings.PostsPerPage);
		Assert.Equal(settings.ForumTitle, String.Empty);
	}

	[Fact]
	public void LoadFromRepo()
	{
		const string tos = "blah blah blah";
		const bool isNewUserApproved = false;
		const int topicsPerPage = 72;
		const int postsPerPage = 42;
		const string title = "superawesome forum";
		var dictionary = new Dictionary<string, string>
		{
			{"TermsOfService", tos},
			{"IsNewUserApproved", isNewUserApproved.ToString()},
			{"TopicsPerPage", topicsPerPage.ToString()},
			{"PostsPerPage", postsPerPage.ToString()},
			{"ForumTitle", title}
		};
		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(dictionary);
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;
		
		Assert.False(settings.IsNewUserApproved);
		Assert.Equal(topicsPerPage, settings.TopicsPerPage);
		Assert.Equal(postsPerPage, settings.PostsPerPage);
		Assert.Equal(title, settings.ForumTitle);
		settingsRepo.Received().Get();
	}

	[Fact]
	public void LoadFromRepoThenCache()
	{
		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(new Dictionary<string, string>());
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;
		var settings2 = settingsManager.Current;
		var settings3 = settingsManager.Current;

		Assert.Equal(settings, settings2);
		Assert.Equal(settings, settings3);
		settingsRepo.Received().Get();
	}

	[Fact]
	public void LoadFromRepoWhenStale()
	{
		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(new Dictionary<string, string>());
		settingsRepo.IsStale(Arg.Any<DateTime>()).Returns(true);
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;
		settings = settingsManager.Current;
		settingsRepo.Received(2).Get();
	}

	[Fact]
	public void DoNotLoadFromRepoWhenNotStale()
	{
		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(new Dictionary<string, string>());
		settingsRepo.IsStale(Arg.Any<DateTime>()).Returns(false);
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;
		settings = settingsManager.Current;
		settingsRepo.Received().Get();
	}

	[Fact]
	public void SaveCurrent()
	{
		const string tos = "blah blah blah";
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
		const bool isPrivateForumInstance = true;
		const string replyToAddress = "D@e.com";
		const int postImageMaxHeight = 654;
		const int postImageMaxWidth = 980;
		const int postImageMaxkBytes = 631;
		var dictionary = new Dictionary<string, object>
		{
			{"TermsOfService", tos},
			{"IsNewUserApproved", isNewUserApproved},
			{"TopicsPerPage", topicsPerPage},
			{"PostsPerPage", postsPerPage},
			{"ForumTitle", title},
			{"MinimumSecondsBetweenPosts", minimumSecondsBetweenPosts},
			{"SmtpServer", smtpServer},
			{"SmtpPort", smtpPort},
			{"MailerAddress", mailerAddress},
			{"ReplyToAddress", replyToAddress},
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
			{"CloseAgedTopicsDays", closeAgedTopicsDays},
			{"IsPrivateForumInstance", isPrivateForumInstance},
			{"PostImageMaxHeight", postImageMaxHeight},
			{"PostImageMaxWidth", postImageMaxWidth},
			{"PostImageMaxkBytes", postImageMaxkBytes}
		};

		var settingsRepo = Substitute.For<ISettingsRepository>();
		settingsRepo.Get().Returns(new Dictionary<string, string>());
		var errorLog = Substitute.For<IErrorLog>();
		var settingsManager = new SettingsManager(settingsRepo, errorLog);

		var settings = settingsManager.Current;
		settings.TermsOfService = tos;
		settings.IsNewUserApproved = isNewUserApproved;
		settings.TopicsPerPage = topicsPerPage;
		settings.PostsPerPage = postsPerPage;
		settings.ForumTitle = title;
		settings.MinimumSecondsBetweenPosts = minimumSecondsBetweenPosts;
		settings.SmtpServer = smtpServer;
		settings.SmtpPort = smtpPort;
		settings.MailerAddress = mailerAddress;
		settings.ReplyToAddress = replyToAddress;
		settings.UseEsmtp = useEsmtp;
		settings.SmtpUser = smtpUser;
		settings.SmtpPassword = smtpPassword;
		settings.MailSendingInverval = mailSendingInverval;
		settings.UseSslSmtp = useSslSmtp;
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
		settings.IsPrivateForumInstance = isPrivateForumInstance;
		settings.PostImageMaxHeight = postImageMaxHeight;
		settings.PostImageMaxWidth = postImageMaxWidth;
		settings.PostImageMaxkBytes = postImageMaxkBytes;
		settingsManager.SaveCurrent();

		settingsRepo.Received().Save(dictionary);
	}
}