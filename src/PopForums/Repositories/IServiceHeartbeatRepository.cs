using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IServiceHeartbeatRepository
	{
		void RecordHeartbeat(string serviceName, string machineName, DateTime lastRun);
		List<ServiceHeartbeat> GetAll();
		void ClearAll();
	}
}