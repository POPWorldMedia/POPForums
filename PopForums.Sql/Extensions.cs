using System;
using System.Data.Common;

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
