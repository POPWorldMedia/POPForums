using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISecurityLogService
	{
		List<SecurityLogEntry> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate);
		List<SecurityLogEntry> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate);
		void CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType);
		void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType);
		void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
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

		public List<SecurityLogEntry> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate)
		{
			return _securityLogRepository.GetByUserID(userID, startDate, endDate);
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			return _securityLogRepository.GetIPHistory(ip, start, end);
		}

		public List<SecurityLogEntry> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate)
		{
			var user = _userRepository.GetUserByName(name);
			if (user == null)
				return new List<SecurityLogEntry>();
			return _securityLogRepository.GetByUserID(user.UserID, startDate, endDate);
		}

		public void CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType)
		{
			if (!string.IsNullOrEmpty(message) && message.Length > 255)
				message = message.Substring(0, 255);
			CreateLogEntry(user?.UserID, targetUser?.UserID, ip, message, securityLogType);
		}

		public void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType)
		{
			CreateLogEntry(userID, targetUserID, ip, message, securityLogType, DateTime.UtcNow);
		}

		public void CreateLogEntry(int? userID, int? targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp)
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
			_securityLogRepository.Create(entry);
		}
	}
}