using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using PopForums.Configuration;

namespace PopForums.Sql
{
	public class SqlObjectFactory : ISqlObjectFactory
	{
		private readonly IConfig _config;

		public SqlObjectFactory(IConfig config)
		{
			_config = config;
		}

		public DbConnection GetConnection()
		{
			if (string.IsNullOrWhiteSpace(_config.DatabaseConnectionString))
				throw new Exception("No database connection string found for POP Forums.");
			var connectionString = _config.DatabaseConnectionString;
            return new SqlConnection(connectionString);
		}

		public DbCommand GetCommand()
		{
			return new SqlCommand();
		}

		public DbCommand GetCommand(string sql)
		{
			return new SqlCommand(sql);
		}

		public DbCommand GetCommand(string sql, DbConnection connection)
		{
			return new SqlCommand(sql, (SqlConnection)connection);
		}

		public DbParameter GetParameter(string parameterName, object value)
		{
			return new SqlParameter(parameterName, value);
		}
	}
}
