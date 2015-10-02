using System;
using System.Data.Common;
using Microsoft.Framework.DependencyInjection;
using PopForums.Configuration;
using PopForums.Data.Sql.Repositories;
using PopForums.Repositories;

namespace PopForums.Data.Sql
{
	public static class Extensions
	{
		public static DbCommand Command(this DbConnection connection, string sql)
		{
			// TODO: get factory from DI container
			//var factory = PopForumsActivation.Container.GetInstance<ISqlObjectFactory>();
			//var command = factory.GetCommand(sql, connection);
			var command = new SqlObjectFactory().GetCommand(sql, connection);
			return command;
		}

		public static DbCommand AddParameter(this DbCommand command, string parameterName, object value)
		{
			// TODO: get factory from DI container
			//var factory = PopForumsActivation.Container.GetInstance<ISqlObjectFactory>();
			//var parameter = factory.GetParameter(parameterName, value);
			var parameter = new SqlObjectFactory().GetParameter(parameterName, value);
            command.Parameters.Add(parameter);
			return command;
		}

		public static object ExecuteAndReturnIdentity(this DbCommand command)
		{
			if (command.Connection == null)
				throw new Exception("SqlCommand has no connection.");
			command.ExecuteNonQuery();
			command.Parameters.Clear();
			command.CommandText = "SELECT @@IDENTITY";
			var result = command.ExecuteScalar();
			return result;
		}

		public static DbDataReader ReadOne(this DbDataReader reader, Action<DbDataReader> action)
		{
			if (reader.Read())
				action(reader);
			reader.Dispose();
			return reader;
		}

		public static DbDataReader ReadAll(this DbDataReader reader, Action<DbDataReader> action)
		{
			while (reader.Read())
				action(reader);
			reader.Dispose();
			return reader;
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

		public static void AddPopForumsSql(this IServiceCollection services)
		{
			services.AddTransient<ICacheHelper, CacheHelper>();
			services.AddTransient<ISqlObjectFactory, SqlObjectFactory>();
			services.AddTransient<IAwardCalculationQueueRepository, AwardCalculationQueueRepository>();
			services.AddTransient<IAwardConditionRepository, AwardConditionRepository>();
			services.AddTransient<IAwardDefinitionRepository, AwardDefinitionRepository>();
			services.AddTransient<IBanRepository, BanRepository>();
			services.AddTransient<ICategoryRepository, CategoryRepository>();
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
			services.AddTransient<ISearchRepository, SearchRepository>();
			services.AddTransient<ISecurityLogRepository, SecurityLogRepository>();
			services.AddTransient<ISettingsRepository, SettingsRepository>();
			services.AddTransient<ISetupRepository, SetupRepository>();
			services.AddTransient<ISubscribedTopicsRepository, SubscribedTopicsRepository>();
			services.AddTransient<ITopicRepository, TopicRepository>();
			services.AddTransient<IUserAvatarRepository, UserAvatarRepository>();
			services.AddTransient<IUserAwardRepository, UserAwardRepository>();
			services.AddTransient<IUserImageRepository, UserImageRepository>();
			services.AddTransient<IUserRepository, UserRepository>();
			services.AddTransient<IUserSessionRepository, UserSessionRepository>();
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

		public static DateTime? NullDateTimeDbHelper(this DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetDateTime(index);
		}

		public static string NullStringDbHelper(this DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetString(index);
		}

		public static Guid? NullGuidDbHelper(this DbDataReader reader, int index)
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
