using PopForums.Configuration;
using PopForums.Data.SqlSingleWebServer.Repositories;
using PopForums.Repositories;
using StructureMap.Configuration.DSL;
using StructureMap.Configuration.DSL.Expressions;

namespace PopForums.Data.SqlSingleWebServer
{
	public class SqlSingleInjectionRegistry : Registry
	{
		public SqlSingleInjectionRegistry()
		{
			For<ICacheHelper>().Use<CacheHelper>();
			For<ISqlObjectFactory>().Use<SqlObjectFactory>();
			For<IUserRepository>().Use<UserRepository>();
			For<IRoleRepository>().Use<RoleRepository>();
			For<IProfileRepository>().Use<ProfileRepository>();
			For<ICategoryRepository>().Use<CategoryRepository>();
			For<IForumRepository>().Use<ForumRepository>();
			For<ITopicRepository>().Use<TopicRepository>();
			For<IPostRepository>().Use<PostRepository>();
			For<ISettingsRepository>().Use<SettingsRepository>();
			For<ISearchRepository>().Use<SearchRepository>();
			For<IUserAvatarRepository>().Use<UserAvatarRepository>();
			For<IUserImageRepository>().Use<UserImageRepository>();
			For<ISecurityLogRepository>().Use<SecurityLogRepository>();
			For<IUserSessionRepository>().Use<UserSessionRepository>();
			For<IErrorLogRepository>().Use<ErrorLogRepository>();
			For<IBanRepository>().Use<BanRepository>();
			For<IQueuedEmailMessageRepository>().Use<QueuedEmailMessageRepository>();
			For<ISubscribedTopicsRepository>().Use<SubscribedTopicsRepository>();
			For<ILastReadRepository>().Use<LastReadRepository>();
			For<IFavoriteTopicsRepository>().Use<FavoriteTopicsRepository>();
			For<IModerationLogRepository>().Use<ModerationLogRepository>();
			For<IPrivateMessageRepository>().Use<PrivateMessageRepository>();
			For<ISetupRepository>().Use<SetupRepository>();
			For<IPointLedgerRepository>().Use<PointLedgerRepository>();
			For<IEventDefinitionRepository>().Use<EventDefinitionRepository>();
			For<IFeedRepository>().Use<FeedRepository>();
			For<IAwardCalculationQueueRepository>().Use<AwardCalculationQueueRepository>();
			For<IAwardDefinitionRepository>().Use<AwardDefinitionRepository>();
			For<IUserAwardRepository>().Use<UserAwardRepository>();
			For<IAwardConditionRepository>().Use<AwardConditionRepository>();
			For<IFriendRepository>().Use<FriendRepository>();
			For<IExternalUserAssociationRepository>().Use<ExternalUserAssociationRepository>();
		}
	}
}