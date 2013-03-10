using Ninject.Modules;
using PopForums.Configuration;
using PopForums.Data.SqlSingleWebServer.Repositories;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer
{
	public class SqlSingleInjectionModule : NinjectModule
	{
		public override void Load()
		{
			Bind<ICacheHelper>().To<CacheHelper>();
			Bind<ISqlObjectFactory>().To<SqlObjectFactory>();
			Bind<IUserRepository>().To<UserRepository>();
			Bind<IRoleRepository>().To<RoleRepository>();
			Bind<IProfileRepository>().To<ProfileRepository>();
			Bind<ICategoryRepository>().To<CategoryRepository>();
			Bind<IForumRepository>().To<ForumRepository>();
			Bind<ITopicRepository>().To<TopicRepository>();
			Bind<IPostRepository>().To<PostRepository>();
			Bind<ISettingsRepository>().To<SettingsRepository>().InSingletonScope();
			Bind<ISearchRepository>().To<SearchRepository>();
			Bind<IUserAvatarRepository>().To<UserAvatarRepository>();
			Bind<IUserImageRepository>().To<UserImageRepository>();
			Bind<ISecurityLogRepository>().To<SecurityLogRepository>();
			Bind<IUserSessionRepository>().To<UserSessionRepository>();
			Bind<IErrorLogRepository>().To<ErrorLogRepository>();
			Bind<IBanRepository>().To<BanRepository>();
			Bind<IQueuedEmailMessageRepository>().To<QueuedEmailMessageRepository>();
			Bind<ISubscribedTopicsRepository>().To<SubscribedTopicsRepository>();
			Bind<ILastReadRepository>().To<LastReadRepository>();
			Bind<IFavoriteTopicsRepository>().To<FavoriteTopicsRepository>();
			Bind<IModerationLogRepository>().To<ModerationLogRepository>();
			Bind<IPrivateMessageRepository>().To<PrivateMessageRepository>();
			Bind<ISetupRepository>().To<SetupRepository>();
			Bind<IPointLedgerRepository>().To<PointLedgerRepository>();
			Bind<IEventDefinitionRepository>().To<EventDefinitionRepository>();
			Bind<IFeedRepository>().To<FeedRepository>();
			Bind<IAwardCalculationQueueRepository>().To<AwardCalculationQueueRepository>();
			Bind<IAwardDefinitionRepository>().To<AwardDefinitionRepository>();
			Bind<IUserAwardRepository>().To<UserAwardRepository>();
			Bind<IAwardConditionRepository>().To<AwardConditionRepository>();
		}
	}
}