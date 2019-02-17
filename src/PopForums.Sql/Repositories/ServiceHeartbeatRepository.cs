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
			// Not crazy about this lock, but there appear to be multiple instances of this running 
			// when in a web context, causing PK violations when the old record isn't deleted. Of course 
			// this goes away when running in Azure functions on a consumption plan.
			lock (_heartbeatlock)
			{
				_sqlObjectFactory.GetConnection().Using(connection =>
				{
					connection.Execute("DELETE FROM pf_ServiceHeartbeat WHERE ServiceName = @ServiceName AND MachineName = @MachineName", new { ServiceName = serviceName, MachineName = machineName });
					connection.Execute("INSERT INTO pf_ServiceHeartbeat (ServiceName, MachineName, LastRun) VALUES (@ServiceName, @MachineName, @LastRun)", new { ServiceName = serviceName, MachineName = machineName, LastRun = lastRun });
				});
			}
		}

		private static object _heartbeatlock = new Object();

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