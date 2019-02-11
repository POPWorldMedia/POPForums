using System.Data.Common;

namespace PopForums.Sql
{
	public interface ISqlObjectFactory
	{
		DbConnection GetConnection();
		DbCommand GetCommand();
		DbCommand GetCommand(string sql);
		DbCommand GetCommand(string sql, DbConnection connection);
		DbParameter GetParameter(string parameterName, object value);
	}
}
