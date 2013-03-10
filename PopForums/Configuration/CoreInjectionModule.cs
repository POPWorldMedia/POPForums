using Ninject.Modules;
using PopForums.Email;
using PopForums.Feeds;
using PopForums.Messaging;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Configuration
{
	public class CoreInjectionModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IUserService>().To<UserService>();
			Bind<IProfileService>().To<ProfileService>();
			Bind<IFormsAuthenticationWrapper>().To<FormsAuthenticationWrapper>();
			Bind<IConfig>().To<Config>();
			Bind<ISettingsManager>().To<SettingsManager>();
			Bind<INewAccountMailer>().To<NewAccountMailer>();
			Bind<ICategoryService>().To<CategoryService>();
			Bind<IForumService>().To<ForumService>();
			Bind<ITopicService>().To<TopicService>();
			Bind<IPostService>().To<PostService>();
			Bind<ITopicViewCountService>().To<TopicViewCountService>();
			Bind<ISmtpWrapper>().To<SmtpWrapper>();
			Bind<ISearchService>().To<SearchService>();
			Bind<IImageService>().To<ImageService>();
			Bind<ITextParsingService>().To<TextParsingService>();
			Bind<ISecurityLogService>().To<SecurityLogService>();
			Bind<IUserSessionService>().To<UserSessionService>();
			Bind<IErrorLog>().To<ErrorLog>();
			Bind<ITimeFormattingService>().To<TimeFormattingService>();
			Bind<IBanService>().To<BanService>().InSingletonScope();
			Bind<ISubscribedTopicsService>().To<SubscribedTopicsService>();
			Bind<ISubscribedTopicEmailComposer>().To<SubscribedTopicEmailComposer>();
			Bind<ILastReadService>().To<LastReadService>();
			Bind<IFavoriteTopicService>().To<FavoriteTopicService>();
			Bind<IModerationLogService>().To<ModerationLogService>();
			Bind<IForgotPasswordMailer>().To<ForgotPasswordMailer>();
			Bind<IClientSettingsMapper>().To<ClientSettingsMapper>();
			Bind<IUserEmailer>().To<UserEmailer>();
			Bind<IIPHistoryService>().To<IPHistoryService>();
			Bind<IPrivateMessageService>().To<PrivateMessageService>();
			Bind<IMobileDetectionWrapper>().To<MobileDetectionWrapper>();
			Bind<IMailingListComposer>().To<MailingListComposer>();
			Bind<IMailingListService>().To<MailingListService>();
			Bind<ISetupService>().To<SetupService>();
			Bind<IEventPublisher>().To<EventPublisher>();
			Bind<IEventDefinitionService>().To<EventDefinitionService>();
			Bind<IAwardCalculator>().To<AwardCalculator>();
			Bind<IFeedService>().To<FeedService>();
			Bind<IAwardDefinitionService>().To<AwardDefinitionService>();
			Bind<IUserAwardService>().To<UserAwardService>();

			Bind<IBroker>().To<Broker>();
		}
	}
}