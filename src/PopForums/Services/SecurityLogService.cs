using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISecurityLogService
	{
		Task<List<SecurityLogEntry>> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate);
		Task<List<SecurityLogEntry>> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate);
		Task CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType);
		Task CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType);
		Task CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp);
		Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end);
	}

	public class SecurityLogService : ISecurityLogService
	{
		public SecurityLogService(ISecurityLogRepository securityLogRepsoitory, IUserRepository userRepository)
		{
			_securityLogRepository = securityLogRepsoitory;
			_userRepository = userRepository;
		}

		private readonly ISecurityLogRepository _securityLogRepository;
		private readonly IUserRepository _userRepository;

		public async Task<List<SecurityLogEntry>> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			return await _securityLogRepository.GetByUserID(userID, startDate, endDate);
		}

		public async Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			return await _securityLogRepository.GetIPHistory(ip, start, end);
		}

		public async Task<List<SecurityLogEntry>> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate)
		{
			var user = await _userRepository.GetUserByName(name);
			if (user == null)
				return new List<SecurityLogEntry>();
			return await _securityLogRepository.GetByUserID(user.UserID, startDate, endDate);
		}

		public async Task CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType)
		{
			if (!string.IsNullOrEmpty(message) && message.Length > 255)
				message = message.Substring(0, 255);
			await CreateLogEntry(user?.UserID, targetUser?.UserID, ip, message, securityLogType);
		}

		public async Task CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType)
		{
			await CreateLogEntry(userID, targetUserID, ip, message, securityLogType, DateTime.UtcNow);
		}

		public async Task CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp)
		{
			if (ip == null)
				throw new ArgumentNullException("ip");
			if (message == null)
				throw new ArgumentNullException("message");
			var entry = new SecurityLogEntry
			{
				UserID = userID,
				TargetUserID = targetUserID,
				ActivityDate = timeStamp,
				IP = ip,
				Message = message,
				SecurityLogType = securityLogType
			};
			await _securityLogRepository.Create(entry);
		}
	}
}