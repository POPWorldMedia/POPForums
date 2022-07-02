namespace PopForums.Configuration;

public class Settings
{
	public Settings()
	{
		TermsOfService = string.Empty;
		IsNewUserApproved = true;
		TopicsPerPage = 20;
		PostsPerPage = 20;
		ForumTitle = string.Empty;
		MinimumSecondsBetweenPosts = 30;
		SmtpServer = "localhost";
		SmtpPort = 25;
		MailerAddress = string.Empty;
		ReplyToAddress = string.Empty;
		UseEsmtp = false;
		SmtpUser = string.Empty;
		SmtpPassword = string.Empty;
		MailSendingInverval = 1500;
		UseSslSmtp = false;
		SessionLength = 20;
		CensorWords = string.Empty;
		CensorCharacter = "*";
		AllowImages = false;
		LogSecurity = true;
		LogModeration = true;
		LogErrors = true;
		IsNewUserImageApproved = false;
		SearchIndexingInterval = 10000;
		IsSearchIndexingEnabled = true;
		IsMailerEnabled = true;
		UserImageMaxHeight = 300;
		UserImageMaxWidth = 400;
		UserImageMaxkBytes = 100;
		UserAvatarMaxHeight = 90;
		UserAvatarMaxWidth = 90;
		UserAvatarMaxkBytes = 10;
		MailSignature = string.Empty;
		ScoringGameCalculatorInterval = 1000;
		MailerQuantity = 4;
		UseGoogleLogin = false;
		UseFacebookLogin = false;
		FacebookAppID = string.Empty;
		FacebookAppSecret = string.Empty;
		UseMicrosoftLogin = false;
		MicrosoftClientID = string.Empty;
		MicrosoftClientSecret = string.Empty;
		YouTubeHeight = 360;
		YouTubeWidth = 640;
		GoogleClientId = string.Empty;
		GoogleClientSecret = string.Empty;
		UseOAuth2Login = false;
		OAuth2ClientID = string.Empty;
		OAuth2ClientSecret = string.Empty;
		OAuth2LoginUrl = string.Empty;
		OAuth2TokenUrl = string.Empty;
		OAuth2DisplayName = string.Empty;
		OAuth2Scope = "email";
		IsClosingAgedTopics = false;
		CloseAgedTopicsDays = 365;
		IsPrivateForumInstance = false;
		PostImageMaxHeight = 1000;
		PostImageMaxWidth = 1000;
		PostImageMaxkBytes = 5000;
	}

	public virtual string TermsOfService { get; set; }
	public virtual bool IsNewUserApproved { get; set; }
	public virtual int TopicsPerPage { get; set; }
	public virtual int PostsPerPage { get; set; }
	public virtual string ForumTitle { get; set; }
	public virtual int MinimumSecondsBetweenPosts { get; set; }
	public virtual string SmtpServer { get; set; }
	public virtual int SmtpPort { get; set; }
	public virtual string MailerAddress { get; set; }
	public virtual string ReplyToAddress { get; set; }
	public virtual bool UseEsmtp { get; set; }
	public virtual string SmtpUser { get; set; }
	public virtual string SmtpPassword { get; set; }
	public virtual int MailSendingInverval { get; set; }
	public virtual bool UseSslSmtp { get; set; }
	public virtual int SessionLength { get; set; }
	public virtual string CensorWords { get; set; }
	public virtual string CensorCharacter { get; set; }
	public virtual bool AllowImages { get; set; }
	public virtual bool LogSecurity { get; set; }
	public virtual bool LogModeration { get; set; }
	public virtual bool LogErrors { get; set; }
	public virtual bool IsNewUserImageApproved { get; set; }
	public virtual int SearchIndexingInterval { get; set; }
	public virtual bool IsSearchIndexingEnabled { get; set; }
	public virtual bool IsMailerEnabled { get; set; }
	public virtual int UserImageMaxHeight { get; set; }
	public virtual int UserImageMaxWidth { get; set; }
	public virtual int UserImageMaxkBytes { get; set; }
	public virtual int UserAvatarMaxHeight { get; set; }
	public virtual int UserAvatarMaxWidth { get; set; }
	public virtual int UserAvatarMaxkBytes { get; set; }
	public virtual string MailSignature { get; set; }
	public virtual int ScoringGameCalculatorInterval { get; set; }
	public virtual int MailerQuantity { get; set; }
	public virtual bool UseGoogleLogin { get; set; }
	public virtual bool UseFacebookLogin { get; set; }
	public virtual string FacebookAppID { get; set; }
	public virtual string FacebookAppSecret { get; set; }
	public virtual bool UseMicrosoftLogin { get; set; }
	public virtual string MicrosoftClientID { get; set; }
	public virtual string MicrosoftClientSecret { get; set; }
	public virtual int YouTubeHeight { get; set; }
	public virtual int YouTubeWidth { get; set; }
	public virtual string GoogleClientId { get; set; }
	public virtual string GoogleClientSecret { get; set; }
	public virtual bool UseOAuth2Login { get; set; }
	public virtual string OAuth2ClientID { get; set; }
	public virtual string OAuth2ClientSecret { get; set; }
	public virtual string OAuth2LoginUrl { get; set; }
	public virtual string OAuth2TokenUrl { get; set; }
	public virtual string OAuth2DisplayName { get; set; }
	public virtual string OAuth2Scope { get; set; }
	public virtual bool IsClosingAgedTopics { get; set; }
	public virtual int CloseAgedTopicsDays { get; set; }
	public virtual bool IsPrivateForumInstance { get; set; }
	public virtual int PostImageMaxHeight { get; set; }
	public virtual int PostImageMaxWidth { get; set; }
	public virtual int PostImageMaxkBytes { get; set; }
}