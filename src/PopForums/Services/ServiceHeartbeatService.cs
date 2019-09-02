using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IServiceHeartbeatService
	{
		Task RecordHeartbeat(string serviceName, string machineName);
		Task<List<ServiceHeartbeat>> GetAll();
		Task ClearAll();
	}

	public class ServiceHeartbeatService : IServiceHeartbeatService
	{
		private readonly IServiceHeartbeatRepository _serviceHeartbeatRepository;

		public ServiceHeartbeatService(IServiceHeartbeatRepository serviceHeartbeatRepository)
		{
			_serviceHeartbeatRepository = serviceHeartbeatRepository;
		}

		public async Task RecordHeartbeat(string serviceName, string machineName)
		{
			await _serviceHeartbeatRepository.RecordHeartbeat(serviceName, machineName, DateTime.UtcNow);
		}

		public async Task<List<ServiceHeartbeat>> GetAll()
		{
			return await _serviceHeartbeatRepository.GetAll();
		}

		public async Task ClearAll()
		{
			await _serviceHeartbeatRepository.ClearAll();
		}
	}
}