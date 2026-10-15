namespace PopForums.Services;

public interface IUserSessionService
{
	Task<int> ProcessUserRequest(User user, int? sessionID, string ip, Action deleteSession, Action<int> createSession);
	Task CleanUpExpiredSessions();
	Task<int> GetTotalSessionCount();
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
			if (user == null)
			{
				// Anonymous: issue the cookie only. The session row is created when the cookie comes back,
				// so clients that don't keep cookies (most bots) never write to the database.
				sessionID = NewSessionID();
				createSession(sessionID.Value);
			}
			else
			{
				sessionID = await StartNewSession(userID, ip, createSession);
				await _userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
			}
		}
		else
		{
			if (user != null)
				await _userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
			var updateSuccess = await _userSessionRepository.UpdateSession(sessionID.Value, DateTime.UtcNow);
			if (!updateSuccess)
				// No row yet (deferred anonymous start) or it expired: start the session under the cookie's ID.
				await StartNewSession(userID, ip, createSession, sessionID.Value);
			else
			{
				var isAnon = await _userSessionRepository.IsSessionAnonymous(sessionID.Value);
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

	private async Task<int> StartNewSession(int? userID, string ip, Action<int> createSession, int? existingSessionID = null)
	{
		if (userID.HasValue)
		{
			var oldUserSession = await _userSessionRepository.GetSessionIDByUserID(userID.Value);
			if (oldUserSession != null)
				await EndAndDeleteSession(oldUserSession);
		}
		var sessionID = existingSessionID ?? NewSessionID();
		await _securityLogService.CreateLogEntry(null, userID, ip, sessionID.ToString(), SecurityLogType.UserSessionStart);
		await _userSessionRepository.CreateSession(sessionID, userID, DateTime.UtcNow);
		if (!existingSessionID.HasValue)
			createSession(sessionID);
		return sessionID;
	}

	private static int NewSessionID()
	{
		var random = new Random();
		return random.Next(int.MinValue, int.MaxValue);
	}

	private async Task EndAndDeleteSession(ExpiredUserSession oldUserSession)
	{
		await _securityLogService.CreateLogEntry(null, oldUserSession.UserID, string.Empty, oldUserSession.SessionID.ToString(), SecurityLogType.UserSessionEnd, oldUserSession.LastTime);
		await _userSessionRepository.DeleteSessions(oldUserSession.UserID, oldUserSession.SessionID);
	}

	public async Task CleanUpExpiredSessions()
	{
		var cutOff = DateTime.UtcNow.Subtract(new TimeSpan(0, _settingsManager.Current.SessionLength, 0));
		var expiredSessions = await _userSessionRepository.GetAndDeleteExpiredSessions(cutOff);
		foreach (var session in expiredSessions)
			await _securityLogService.CreateLogEntry(null, session.UserID, string.Empty, session.SessionID.ToString(), SecurityLogType.UserSessionEnd, session.LastTime);
	}

	public async Task<int> GetTotalSessionCount()
	{
		return await _userSessionRepository.GetTotalSessionCount();
	}
}