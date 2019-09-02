using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task RecordHeartbeat(string serviceName, string machineName, DateTime lastRun)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(async connection =>
			{
				await connection.ExecuteAsync("DELETE FROM pf_ServiceHeartbeat WHERE ServiceName = @ServiceName AND MachineName = @MachineName", new { ServiceName = serviceName, MachineName = machineName });
				await connection.ExecuteAsync("INSERT INTO pf_ServiceHeartbeat (ServiceName, MachineName, LastRun) VALUES (@ServiceName, @MachineName, @LastRun)", new { ServiceName = serviceName, MachineName = machineName, LastRun = lastRun });
			});
		}

		public async Task<List<ServiceHeartbeat>> GetAll()
		{
			Task<IEnumerable<ServiceHeartbeat>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				list = connection.QueryAsync<ServiceHeartbeat>("SELECT ServiceName, MachineName, LastRun FROM pf_ServiceHeartbeat ORDER BY ServiceName"));
			return list.Result.ToList();
		}

		public async Task ClearAll()
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("DELETE FROM pf_ServiceHeartbeat"));
		}
	}
}