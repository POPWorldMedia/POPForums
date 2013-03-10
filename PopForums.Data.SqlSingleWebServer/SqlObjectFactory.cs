using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using PopForums.Configuration;

namespace PopForums.Data.SqlSingleWebServer
{
	public class SqlObjectFactory : ISqlObjectFactory
	{
		public IDbConnection GetConnection()
		{
			var config = new Config();
			if (ConfigurationManager.ConnectionStrings[config.ConnectionStringName] == null)
				throw new Exception(String.Format("Can't find a connection string for PopForums by the name \"{0}\"", config.ConnectionStringName));
			var connectionString = ConfigurationManager.ConnectionStrings[config.ConnectionStringName].ConnectionString;
			return new SqlConnection(connectionString);
		}

		public IDbCommand GetCommand()
		{
			return new SqlCommand();
		}

		public IDbCommand GetCommand(string sql)
		{
			return new SqlCommand(sql);
		}

		public IDbCommand GetCommand(string sql, IDbConnection connection)
		{
			return new SqlCommand(sql, (SqlConnection)connection);
		}

		public DbParameter GetParameter(string parameterName, object value)
		{
			return new SqlParameter(parameterName, value);
		}
	}
}
