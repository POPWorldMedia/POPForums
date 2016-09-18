using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
			_sqlObjectFactory.GetConnection()
				.Using(connection => connection.Command("DELETE FROM pf_ServiceHeartbeat WHERE ServiceName = @ServiceName AND MachineName = @MachineName")
					.AddParameter("@ServiceName", serviceName)
					.AddParameter("@MachineName", machineName)
					.ExecuteNonQuery());

			_sqlObjectFactory.GetConnection()
				.Using(connection => connection.Command("INSERT INTO pf_ServiceHeartbeat (ServiceName, MachineName, LastRun) VALUES (@ServiceName, @MachineName, @LastRun)")
					.AddParameter("@ServiceName", serviceName)
					.AddParameter("@MachineName", machineName)
					.AddParameter("@LastRun", lastRun)
					.ExecuteNonQuery());
		}

		public List<ServiceHeartbeat> GetAll()
		{
			var list = new List<ServiceHeartbeat>();
			_sqlObjectFactory.GetConnection()
				.Using(connection => connection.Command("SELECT ServiceName, MachineName, LastRun FROM pf_ServiceHeartbeat ORDER BY ServiceName")
					.ExecuteReader()
					.ReadAll(r => list.Add(new ServiceHeartbeat
					{
						ServiceName = r.GetString(0),
						MachineName = r.GetString(1),
						LastRun = r.GetDateTime(2)
					})));
			return list;
		}
	}
}