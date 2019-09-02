using System;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IUserSessionService
	{
		Task<int> ProcessUserRequest(User user, int? sessionID, string ip, Action deleteSession, Action<int> createSession);
		Task CleanUpExpiredSessions();
		int GetTotalSessionCount();
	}

	public class UserSessionService : IUserSessionService
	{
		public UserSessionService(ISettingsManager settingsManager, IUserRepository userRepository, IUserSessionRepository userSessionRepository, ISecurityLogService securityLogService)
		{
			_settingsManager = settingsManager;
			_userRepository = userRepository;
			_userSessionRepository = userSessionRepository;
			_securityLogService = securityLogService;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IUserRepository _userRepository;
		private readonly IUserSessionRepository _userSessionRepository;
		private readonly ISecurityLogService _securityLogService;

		public const string _sessionIDCookieName = "pf_sessionID";

		public async Task<int> ProcessUserRequest(User user, int? sessionID, string ip, Action deleteSession, Action<int> createSession)
		{
			int? userID = null;
			if (user != null)
				userID = user.UserID;
			if (sessionID == null)
			{
				sessionID = await StartNewSession(userID, ip, createSession);
				if (user != null)
					_userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
			}
			else
			{
				if (user != null)
					_userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
				var updateSuccess = _userSessionRepository.UpdateSession(sessionID.Value, DateTime.UtcNow);
				if (!updateSuccess)
					sessionID = await StartNewSession(userID, ip, createSession);
				else
				{
					var isAnon = _userSessionRepository.IsSessionAnonymous(sessionID.Value);
					if (userID.HasValue && isAnon || !userID.HasValue && !isAnon)
					{
						deleteSession();
						await EndAndDeleteSession(new ExpiredUserSession { UserID = null, SessionID = sessionID.Value, LastTime = DateTime.UtcNow });
						sessionID = await StartNewSession(userID, ip, createSession);
					}
				}
			}
			return sessionID.Value;
		}

		private async Task<int> StartNewSession(int? userID, string ip, Action<int> createSession)
		{
			if (userID.HasValue)
			{
				var oldUserSession = _userSessionRepository.GetSessionIDByUserID(userID.Value);
				if (oldUserSession != null)
					await EndAndDeleteSession(oldUserSession);
			}
			var random = new Random();
			var sessionID = random.Next(int.MinValue, int.MaxValue);
			await _securityLogService.CreateLogEntry(null, userID, ip, sessionID.ToString(), SecurityLogType.UserSessionStart);
			_userSessionRepository.CreateSession(sessionID, userID, DateTime.UtcNow);
			createSession(sessionID);
			return sessionID;
		}

		private async Task EndAndDeleteSession(ExpiredUserSession oldUserSession)
		{
			await _securityLogService.CreateLogEntry(null, oldUserSession.UserID, String.Empty, oldUserSession.SessionID.ToString(), SecurityLogType.UserSessionEnd, oldUserSession.LastTime);
			_userSessionRepository.DeleteSessions(oldUserSession.UserID, oldUserSession.SessionID);
		}

		public async Task CleanUpExpiredSessions()
		{
			var cutOff = DateTime.UtcNow.Subtract(new TimeSpan(0, _settingsManager.Current.SessionLength, 0));
			var expiredSessions = _userSessionRepository.GetAndDeleteExpiredSessions(cutOff);
			foreach (var session in expiredSessions)
				await _securityLogService.CreateLogEntry(null, session.UserID, String.Empty, session.SessionID.ToString(), SecurityLogType.UserSessionEnd, session.LastTime);
		}

		public int GetTotalSessionCount()
		{
			return _userSessionRepository.GetTotalSessionCount();
		}
	}
}