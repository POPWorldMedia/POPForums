using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IServiceHeartbeatService
	{
		void RecordHeartbeat(string serviceName, string machineName);
		List<ServiceHeartbeat> GetAll();
		void ClearAll();
	}

	public class ServiceHeartbeatService : IServiceHeartbeatService
	{
		private readonly IServiceHeartbeatRepository _serviceHeartbeatRepository;

		public ServiceHeartbeatService(IServiceHeartbeatRepository serviceHeartbeatRepository)
		{
			_serviceHeartbeatRepository = serviceHeartbeatRepository;
		}

		public void RecordHeartbeat(string serviceName, string machineName)
		{
			_serviceHeartbeatRepository.RecordHeartbeat(serviceName, machineName, DateTime.UtcNow);
		}

		public List<ServiceHeartbeat> GetAll()
		{
			return _serviceHeartbeatRepository.GetAll();
		}

		public void ClearAll()
		{
			_serviceHeartbeatRepository.ClearAll();
		}
	}
}