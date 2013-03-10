using System.Data;
using System.Data.Common;

namespace PopForums.Configuration
{
	public interface ISqlObjectFactory
	{
		IDbConnection GetConnection();
		IDbCommand GetCommand();
		IDbCommand GetCommand(string sql);
		IDbCommand GetCommand(string sql, IDbConnection connection);
		DbParameter GetParameter(string parameterName, object value);
	}
}
