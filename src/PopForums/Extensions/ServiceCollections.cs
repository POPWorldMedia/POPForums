namespace PopForums.Extensions;

public static class ServiceCollections
{
	public static void AddPopForumsBase(this IServiceCollection services)
	{
		// config
		services.AddTransient<IConfig, Config>();
		services.AddTransient<IErrorLog, ErrorLog>();
		services.AddTransient<ISettingsManager, SettingsManager>();
		services.AddTransient<ITenantService, TenantService>();

		// composers
		services.AddTransient<ITopicStateComposer, TopicStateComposer>();
		services.AddTransient<IForumStateComposer, ForumStateComposer>();

		// email
		services.AddTransient<IForgotPasswordMailer, ForgotPasswordMailer>();
		services.AddTransient<IMailingListComposer, MailingListComposer>();
		services.AddTransient<INewAccountMailer, NewAccountMailer>();
		services.AddTransient<ISmtpWrapper, SmtpWrapper>();
		services.AddTransient<IUserEmailer, UserEmailer>();

		// external auth?
		services.AddTransient<IExternalUserAssociationManager, ExternalUserAssociationManager>();

		// feeds
		services.AddTransient<IFeedService, FeedService>();

		// scoring game
		services.AddTransient<IAwardCalculator, AwardCalculator>();
		services.AddTransient<IAwardDefinitionService, AwardDefinitionService>();
		services.AddTransient<IEventDefinitionService, EventDefinitionService>();
		services.AddTransient<IEventPublisher, EventPublisher>();
		services.AddTransient<IUserAwardService, UserAwardService>();

		// services
		services.AddTransient<IBanService, BanService>();
		services.AddTransient<ICategoryService, CategoryService>();
		services.AddTransient<IFavoriteTopicService, FavoriteTopicService>();
		services.AddTransient<IForumService, ForumService>();
		services.AddTransient<IImageService, ImageService>();
		services.AddTransient<IIPHistoryService, IPHistoryService>();
		services.AddTransient<ILastReadService, LastReadService>();
		services.AddTransient<IMailingListService, MailingListService>();
		services.AddTransient<IModerationLogService, ModerationLogService>();
		services.AddTransient<IPostService, PostService>();
		services.AddTransient<IPrivateMessageService, PrivateMessageService>();
		services.AddTransient<IProfileService, ProfileService>();
		services.AddTransient<IQueuedEmailService, QueuedEmailService>();
		services.AddTransient<ISearchService, SearchService>();
		services.AddTransient<ISecurityLogService, SecurityLogService>();
		services.AddTransient<ISetupService, SetupService>();
		services.AddTransient<ISubscribedTopicsService, SubscribedTopicsService>();
		services.AddTransient<ITextParsingService, TextParsingService>();
		services.AddTransient<ITopicService, TopicService>();
		services.AddTransient<ITopicViewLogService, TopicViewLogService>();
		services.AddTransient<IUserService, UserService>();
		services.AddTransient<IUserSessionService, UserSessionService>();
		services.AddTransient<IServiceHeartbeatService, ServiceHeartbeatService>();
		services.AddTransient<ISearchIndexSubsystem, SearchIndexSubsystem>();
		services.AddTransient<IPostMasterService, PostMasterService>();
		services.AddTransient<IForumPermissionService, ForumPermissionService>();
		services.AddTransient<IReCaptchaService, ReCaptchaService>();
		services.AddTransient<ISitemapService, SitemapService>();
		services.AddTransient<ITimeFormatStringService, TimeFormatStringService>();
		services.AddTransient<IResourceComposer, ResourceComposer>();
		services.AddTransient<INotificationManager, NotificationManager>();
		services.AddTransient<INotificationAdapter, NotificationAdapter>();
		services.AddTransient<INotificationTunnel, NotificationTunnel>();
		services.AddTransient<IPostImageService, PostImageService>();
	}

	public static void AddPopForumsBackgroundServices(this IServiceCollection services)
	{
		var serviceProvider = services.BuildServiceProvider();
		BackgroundServices.SetupServices(serviceProvider);
	}

}