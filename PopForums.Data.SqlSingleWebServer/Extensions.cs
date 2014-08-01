using System;
using System.Data;
using PopForums.Configuration;
using PopForums.Web;

namespace PopForums.Data.SqlSingleWebServer
{
	public static class Extensions
	{
		public static IDbCommand Command(this IDbConnection connection, string sql)
		{
			var factory = PopForumsActivation.Container.GetInstance<ISqlObjectFactory>();
			var command = factory.GetCommand(sql, connection);
			return command;
		}

		public static IDbCommand AddParameter(this IDbCommand command, string parameterName, object value)
		{
			var factory = PopForumsActivation.Container.GetInstance<ISqlObjectFactory>();
			var parameter = factory.GetParameter(parameterName, value);
			command.Parameters.Add(parameter);
			return command;
		}

		public static object ExecuteAndReturnIdentity(this IDbCommand command)
		{
			if (command.Connection == null)
				throw new Exception("SqlCommand has no connection.");
			command.ExecuteNonQuery();
			command.Parameters.Clear();
			command.CommandText = "SELECT @@IDENTITY";
			var result = command.ExecuteScalar();
			return result;
		}

		public static IDataReader ReadOne(this IDataReader reader, Action<IDataReader> action)
		{
			if (reader.Read())
				action(reader);
			reader.Close();
			return reader;
		}

		public static IDataReader ReadAll(this IDataReader reader, Action<IDataReader> action)
		{
			while (reader.Read())
				action(reader);
			reader.Close();
			return reader;
		}

		public static void Using(this IDbConnection connection, Action<IDbConnection> action)
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

		public static object GetObjectOrDbNull(this object value)
		{
			return value ?? DBNull.Value;
		}

		public static int? NullIntDbHelper(this IDataRecord reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetInt32(index);
		}

		public static DateTime? NullDateTimeDbHelper(this IDataRecord reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetDateTime(index);
		}

		public static string NullStringDbHelper(this IDataRecord reader, int index)
		{
			if (reader.IsDBNull(index)) return null;
			return reader.GetString(index);
		}

		public static Guid? NullGuidDbHelper(this IDataRecord reader, int index)
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
