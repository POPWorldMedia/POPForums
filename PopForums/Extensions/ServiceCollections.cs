using Microsoft.Framework.DependencyInjection;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.ExternalLogin;
using PopForums.Feeds;
using PopForums.Messaging;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Extensions
{
    public static class ServiceCollections
    {
	    public static void AddPopForumsBase(this IServiceCollection services)
	    {
		    // config
		    services.AddTransient<IConfig, Config>();
		    services.AddTransient<IErrorLog, ErrorLog>();
		    services.AddTransient<ISettingsManager, SettingsManager>();

			// email
		    services.AddTransient<IForgotPasswordMailer, ForgotPasswordMailer>();
		    services.AddTransient<IMailingListComposer, MailingListComposer>();
		    services.AddTransient<INewAccountMailer, NewAccountMailer>();
		    services.AddTransient<ISmtpWrapper, SmtpWrapper>();
		    services.AddTransient<ISubscribedTopicEmailComposer, SubscribedTopicEmailComposer>();
		    services.AddTransient<IUserEmailer, UserEmailer>();

			// external auth?
		    services.AddTransient<IUserAssociationManager, UserAssociationManager>();

			// feeds
		    services.AddTransient<IFeedService, FeedService>();

			// messaging
		    services.AddTransient<IBroker, Broker>();

			// scoring game
		    services.AddTransient<IAwardCalculator, AwardCalculator>();
		    services.AddTransient<IAwardDefinitionService, AwardDefinitionService>();
		    services.AddTransient<IEventDefinitionService, EventDefinitionService>();
		    services.AddTransient<IEventPublisher, EventPublisher>();
		    services.AddTransient<IUserAwardService, UserAwardService>();

			// services
		    services.AddTransient<IBanService, BanService>();
		    services.AddTransient<ICategoryService, CategoryService>();
		    services.AddTransient<IClientSettingsMapper, ClientSettingsMapper>();
		    services.AddTransient<IFavoriteTopicService, FavoriteTopicService>();
		    //services.AddTransient<IForumAdapterFactory, ForumAdapterFactory>();
		    services.AddTransient<IForumService, ForumService>();
		    services.AddTransient<IFriendService, FriendService>();
		    services.AddTransient<IImageService, ImageService>();
		    services.AddTransient<IIPHistoryService, IPHistoryService>();
		    services.AddTransient<ILastReadService, LastReadService>();
		    services.AddTransient<IMailingListService, MailingListService>();
		    services.AddTransient<IModerationLogService, ModerationLogService>();
		    services.AddTransient<IPostService, PostService>();
		    services.AddTransient<IPrivateMessageService, PrivateMessageService>();
		    services.AddTransient<IProfileService, ProfileService>();
		    services.AddTransient<ISearchService, SearchService>();
		    services.AddTransient<ISecurityLogService, SecurityLogService>();
		    services.AddTransient<ISetupService, SetupService>();
		    services.AddTransient<ISubscribedTopicsService, SubscribedTopicsService>();
		    services.AddTransient<ITextParsingService, TextParsingService>();
		    services.AddTransient<ITimeFormattingService, TimeFormattingService>();
		    services.AddTransient<ITopicService, TopicService>();
		    //services.AddTransient<ITopicViewCountService, TopicViewCountService>();
		    services.AddTransient<IUserService, UserService>();
		    services.AddTransient<IUserSessionService, UserSessionService>();
	    }
    }
}
