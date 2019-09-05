using System;

namespace PopForums.Configuration
{
	public class Settings
	{
		public Settings()
		{
			TermsOfService = String.Empty;
			ServerTimeZone = -5;
			ServerDaylightSaving = true;
			IsNewUserApproved = true;
			TopicsPerPage = 20;
			PostsPerPage = 20;
			ForumTitle = String.Empty;
			MinimumSecondsBetweenPosts = 30;
			SmtpServer = "localhost";
			SmtpPort = 25;
			MailerAddress = String.Empty;
			UseEsmtp = false;
			SmtpUser = String.Empty;
			SmtpPassword = String.Empty;
			MailSendingInverval = 1500;
			UseSslSmtp = false;
			SessionLength = 20;
			CensorWords = String.Empty;
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
			MailSignature = String.Empty;
			ScoringGameCalculatorInterval = 1000;
			MailerQuantity = 4;
			UseGoogleLogin = false;
			UseFacebookLogin = false;
			FacebookAppID = String.Empty;
			FacebookAppSecret = String.Empty;
			UseMicrosoftLogin = false;
			MicrosoftClientID = String.Empty;
			MicrosoftClientSecret = String.Empty;
			YouTubeHeight = 360;
			YouTubeWidth = 640;
			GoogleClientId = String.Empty;
			GoogleClientSecret = String.Empty;
			UseOAuth2Login = false;
			OAuth2ClientID = string.Empty;
			OAuth2ClientSecret = string.Empty;
			OAuth2LoginUrl = string.Empty;
			OAuth2TokenUrl = string.Empty;
			OAuth2DisplayName = string.Empty;
			OAuth2Scope = "email";
			IsClosingAgedTopics = false;
			CloseAgedTopicsDays = 365;
		}

		public virtual string TermsOfService { get; set; }
		public virtual int ServerTimeZone { get; set; }
		public virtual bool ServerDaylightSaving { get; set; }
		public virtual bool IsNewUserApproved { get; set; }
		public virtual int TopicsPerPage { get; set; }
		public virtual int PostsPerPage { get; set; }
		public virtual string ForumTitle { get; set; }
		public virtual int MinimumSecondsBetweenPosts { get; set; }
		public virtual string SmtpServer { get; set; }
		public virtual int SmtpPort { get; set; }
		public virtual string MailerAddress { get; set; }
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
	}
}