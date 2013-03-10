using System;
using System.Web;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
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
		private const string _sessionIDCookieName = "pf_sessionID";

		public void ProcessUserRequest(User user, HttpContextBase context)
		{
			if (context.Response.IsRequestBeingRedirected)
				return;
			int? userID = null;
			if (user != null)
				userID = user.UserID;
			if (context.Request.Cookies[_sessionIDCookieName] == null)
			{
				StartNewSession(context, userID);
				if (user != null)
					_userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
			}
			else
			{
				var sessionID = Convert.ToInt32(context.Request.Cookies[_sessionIDCookieName].Value);
				if (user != null)
					_userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
				var updateSuccess = _userSessionRepository.UpdateSession(sessionID, DateTime.UtcNow);
				if (!updateSuccess)
					StartNewSession(context, userID);
				else
				{
					var isAnon = _userSessionRepository.IsSessionAnonymous(sessionID);
					if (userID.HasValue && isAnon || !userID.HasValue && !isAnon)
					{
						EndAndDeleteSession(new ExpiredUserSession { UserID = null, SessionID = sessionID, LastTime = DateTime.UtcNow });
						StartNewSession(context, userID);
					}
				}
			}
		}

		private void StartNewSession(HttpContextBase context, int? userID)
		{
			if (userID.HasValue)
			{
				var oldUserSession = _userSessionRepository.GetSessionIDByUserID(userID.Value);
				if (oldUserSession != null)
					EndAndDeleteSession(oldUserSession);
			}
			var random = new Random();
			var sessionID = random.Next(int.MinValue, int.MaxValue);
			context.Response.Cookies.Set(new HttpCookie(_sessionIDCookieName, sessionID.ToString()));
			_securityLogService.CreateLogEntry(null, userID, context.Request.UserHostAddress, sessionID.ToString(), SecurityLogType.UserSessionStart);
			_userSessionRepository.CreateSession(sessionID, userID, DateTime.UtcNow);
		}

		private void EndAndDeleteSession(ExpiredUserSession oldUserSession)
		{
			_securityLogService.CreateLogEntry(null, oldUserSession.UserID, String.Empty, oldUserSession.SessionID.ToString(), SecurityLogType.UserSessionEnd, oldUserSession.LastTime);
			_userSessionRepository.DeleteSessions(oldUserSession.UserID, oldUserSession.SessionID);
		}

		public void CleanUpExpiredSessions()
		{
			var cutOff = DateTime.UtcNow.Subtract(new TimeSpan(0, _settingsManager.Current.SessionLength, 0));
			var expiredSessions = _userSessionRepository.GetAndDeleteExpiredSessions(cutOff);
			foreach (var session in expiredSessions)
				_securityLogService.CreateLogEntry(null, session.UserID, String.Empty, session.SessionID.ToString(), SecurityLogType.UserSessionEnd, session.LastTime);
		}

		public int GetTotalSessionCount()
		{
			return _userSessionRepository.GetTotalSessionCount();
		}
	}
}