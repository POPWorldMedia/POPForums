using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IServiceHeartbeatRepository
	{
		Task RecordHeartbeat(string serviceName, string machineName, DateTime lastRun);
		Task<List<ServiceHeartbeat>> GetAll();
		Task ClearAll();
	}
}