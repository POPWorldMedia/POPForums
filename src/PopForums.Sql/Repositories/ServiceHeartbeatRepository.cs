using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class ServiceHeartbeatRepository : IServiceHeartbeatRepository
	{
		public ServiceHeartbeatRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}
		
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public void RecordHeartbeat(string serviceName, string machineName, DateTime lastRun)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
			{
				connection.Execute("DELETE FROM pf_ServiceHeartbeat WHERE ServiceName = @ServiceName AND MachineName = @MachineName", new { ServiceName = serviceName, MachineName = machineName });
				connection.Execute("INSERT INTO pf_ServiceHeartbeat (ServiceName, MachineName, LastRun) VALUES (@ServiceName, @MachineName, @LastRun)", new { ServiceName = serviceName, MachineName = machineName, LastRun = lastRun });
			});
		}

		public List<ServiceHeartbeat> GetAll()
		{
			List<ServiceHeartbeat> list = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				list = connection.Query<ServiceHeartbeat>("SELECT ServiceName, MachineName, LastRun FROM pf_ServiceHeartbeat ORDER BY ServiceName").ToList());
			return list;
		}

		public void ClearAll()
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("DELETE FROM pf_ServiceHeartbeat"));
		}
	}
}