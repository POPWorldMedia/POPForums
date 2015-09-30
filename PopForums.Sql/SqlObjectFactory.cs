using System;
using System.Data.Common;
using System.Data.SqlClient;
using PopForums.Configuration;

namespace PopForums.Data.Sql
{
	public class SqlObjectFactory : ISqlObjectFactory
	{
		public DbConnection GetConnection()
		{
			var config = new Config();
			//if (ConfigurationManager.ConnectionStrings[config.ConnectionStringName] == null)
			//	throw new Exception(String.Format("Can't find a connection string for PopForums by the name \"{0}\"", config.ConnectionStringName));
			//var connectionString = ConfigurationManager.ConnectionStrings[config.ConnectionStringName].ConnectionString;
			// TODO: connectionstring
			var connectionString = "";
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
