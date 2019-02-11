using System;
using System.Data.Common;
using System.Data.SqlClient;
using PopForums.Configuration;

namespace PopForums.Sql
{
	public class SqlObjectFactory : ISqlObjectFactory
	{
		public DbConnection GetConnection()
		{
			var config = new Config();
			if (String.IsNullOrWhiteSpace(config.DatabaseConnectionString))
				throw new Exception("No database connection string found for POP Forums.");
			var connectionString = config.DatabaseConnectionString;
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
