using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.Sql.Repositories;
using PopForums.Repositories;

namespace PopForums.Sql
{
	public static class Extensions
	{
		public static DbCommand Command(this DbConnection connection, ISqlObjectFactory sqlObjectFactory, string sql)
		{
			var command = sqlObjectFactory.GetCommand(sql, connection);
			return command;
		}

		public static DbCommand AddParameter(this DbCommand command, ISqlObjectFactory sqlObjectFactory, string parameterName, object value)
		{
			var parameter = sqlObjectFactory.GetParameter(parameterName, value);
            command.Parameters.Add(parameter);
			return command;
		}

		public static void Using(this DbConnection connection, Action<DbConnection> action)
		{
			using (connection)
			{
				try
				{
					connection.Open();
					action(connection);
				}
				finally
				{
					connection.Close();
					connection.Dispose();
				}
			}
		}

		public static async Task UsingAsync(this DbConnection connection, Func<DbConnection, Task> action)
		{
			using (connection)
			{
				try
				{
					await connection.OpenAsync();
					await action(connection);
				}
				finally
				{
					connection.Close();
					connection.Dispose();
				}
			}
		}

		public static void AddPopForumsSql(this IServiceCollection services)
		{
			services.AddTransient<ICacheHelper, CacheHelper>();
			services.AddTransient<ISqlObjectFactory, SqlObjectFactory>();
			services.AddTransient<IAwardCalculationQueueRepository, AwardCalculationQueueRepository>();
			services.AddTransient<IAwardConditionRepository, AwardConditionRepository>();
			services.AddTransient<IAwardDefinitionRepository, AwardDefinitionRepository>();
			services.AddTransient<IBanRepository, BanRepository>();
			services.AddTransient<ICategoryRepository, CategoryRepository>();
			services.AddTransient<IEmailQueueRepository, EmailQueueRepository>();
			services.AddTransient<IErrorLogRepository, ErrorLogRepository>();
			services.AddTransient<IEventDefinitionRepository, EventDefinitionRepository>();
			services.AddTransient<IExternalUserAssociationRepository, ExternalUserAssociationRepository>();
			services.AddTransient<IFavoriteTopicsRepository, FavoriteTopicsRepository>();
			services.AddTransient<IFeedRepository, FeedRepository>();
			services.AddTransient<IForumRepository, ForumRepository>();
			services.AddTransient<IFriendRepository, FriendRepository>();
			services.AddTransient<ILastReadRepository, LastReadRepository>();
			services.AddTransient<IModerationLogRepository, ModerationLogRepository>();
			services.AddTransient<IPointLedgerRepository, PointLedgerRepository>();
			services.AddTransient<IPostRepository, PostRepository>();
			services.AddTransient<IPrivateMessageRepository, PrivateMessageRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IQueuedEmailMessageRepository, QueuedEmailMessageRepository>();
			services.AddTransient<IRoleRepository, RoleRepository>();
			services.AddTransient<ISearchIndexQueueRepository, SearchIndexQueueRepository>();
			services.AddTransient<ISearchRepository, SearchRepository>();
			services.AddTransient<ISecurityLogRepository, SecurityLogRepository>();
			services.AddTransient<ISettingsRepository, SettingsRepository>();
			services.AddTransient<ISetupRepository, SetupRepository>();
			services.AddTransient<ISubscribedTopicsRepository, SubscribedTopicsRepository>();
			services.AddTransient<ITopicRepository, TopicRepository>();
			services.AddTransient<ITopicViewLogRepository, TopicViewLogRepository>();
			services.AddTransient<IUserAvatarRepository, UserAvatarRepository>();
			services.AddTransient<IUserAwardRepository, UserAwardRepository>();
			services.AddTransient<IUserImageRepository, UserImageRepository>();
			services.AddTransient<IUserRepository, UserRepository>();
			services.AddTransient<IUserSessionRepository, UserSessionRepository>();
			services.AddTransient<IServiceHeartbeatRepository, ServiceHeartbeatRepository>();
		}

		public static object GetObjectOrDbNull(this object value)
		{
			return value ?? DBNull.Value;
		}

		public static int? NullIntDbHelper(this DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetInt32(index);
		}

		public static string NullStringDbHelper(this DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetString(index);
		}

		public static Guid? NullGuidDbHelper(this IDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetGuid(index);
		}

		public static string NullToEmpty(this string text)
		{
			if (String.IsNullOrEmpty(text))
				return String.Empty;
			return text;
		}
	}
}
