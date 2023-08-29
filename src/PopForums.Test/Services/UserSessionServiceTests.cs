namespace PopForums.Test.Services;

public class UserSessionServiceTests
{
	private ISettingsManager _mockSettingsManager;
	private IUserRepository _mockUserRepo;
	private IUserSessionRepository _mockUserSessionRepo;
	private ISecurityLogService _mockSecurityLogService;

	private UserSessionService GetService()
	{
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		_mockUserRepo = Substitute.For<IUserRepository>();
		_mockUserSessionRepo = Substitute.For<IUserSessionRepository>();
		_mockSecurityLogService = Substitute.For<ISecurityLogService>();
		var service = new UserSessionService(_mockSettingsManager, _mockUserRepo, _mockUserSessionRepo, _mockSecurityLogService);
		return service;
	}

	[Fact]
	public async Task AnonUserNoCookieGetsCookieAndSessionStart()
	{
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };

		await service.ProcessUserRequest(null, null, "1.1.1.1", delete, create);
			
		Assert.False(deleteCalled);
		Assert.True(createResult.HasValue);
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), null, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry((int?)null, null, "1.1.1.1", createResult.Value.ToString(), SecurityLogType.UserSessionStart);
		await _mockUserRepo.DidNotReceive().UpdateLastActivityDate(Arg.Any<User>(), Arg.Any<DateTime>());
	}

	[Fact]
	public async Task AnonUserWithCookieUpdateSession()
	{
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.UpdateSession(sessionID, Arg.Any<DateTime>()).Returns(Task.FromResult(true));
		_mockUserSessionRepo.IsSessionAnonymous(sessionID).Returns(Task.FromResult(true));

		var result = await service.ProcessUserRequest(null, sessionID, "1.1.1.1", delete, create);

		Assert.False(deleteCalled);
		Assert.Equal(sessionID, result);
		await _mockUserSessionRepo.Received().UpdateSession(sessionID, Arg.Any<DateTime>());
		await _mockUserRepo.DidNotReceive().UpdateLastActivityDate(Arg.Any<User>(), Arg.Any<DateTime>());
	}

	[Fact]
	public async Task UserWithAnonCookieStartsLoggedInSession()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.UpdateSession(sessionID, Arg.Any<DateTime>()).Returns(Task.FromResult(true));
		_mockUserSessionRepo.IsSessionAnonymous(sessionID).Returns(Task.FromResult(true));

		var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);

		Assert.True(deleteCalled);
		Assert.Equal(createResult, result);
		await _mockUserSessionRepo.Received().UpdateSession(sessionID, Arg.Any<DateTime>());
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
		await _mockUserSessionRepo.Received().DeleteSessions(null, sessionID);
		await _mockSecurityLogService.Received().CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, Arg.Any<DateTime>());
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), user.UserID, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, Arg.Any<string>(), Arg.Any<string>(), SecurityLogType.UserSessionStart);
	}

	[Fact]
	public async Task AnonUserWithLoggedInSessionEndsOldOneStartsNewOne()
	{
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.UpdateSession(sessionID, Arg.Any<DateTime>()).Returns(Task.FromResult(true));
		_mockUserSessionRepo.IsSessionAnonymous(sessionID).Returns(Task.FromResult(false));

		var result = await service.ProcessUserRequest(null, sessionID, "1.1.1.1", delete, create);

		Assert.True(deleteCalled);
		Assert.Equal(createResult, result);
		await _mockUserSessionRepo.Received().UpdateSession(sessionID, Arg.Any<DateTime>());
		await _mockUserSessionRepo.Received().DeleteSessions(null, sessionID);
		await _mockSecurityLogService.Received().CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, Arg.Any<DateTime>());
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), null, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry((int?)null, null, Arg.Any<string>(), Arg.Any<string>(), SecurityLogType.UserSessionStart);
	}

	[Fact]
	public async Task UserWithNoCookieGetsCookieAndSessionStart()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		_mockUserSessionRepo.GetSessionIDByUserID(Arg.Any<int>()).Returns((ExpiredUserSession)null);

		var result = await service.ProcessUserRequest(user, null, "1.1.1.1", delete, create);

		Assert.False(deleteCalled);
		Assert.Equal(createResult, result);
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), 123, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, Arg.Any<string>(), result.ToString(), SecurityLogType.UserSessionStart);
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task UserWithCookieUpdateSession()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		var deleteCalled = false;
		Action delete = () => { deleteCalled = true; };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.UpdateSession(sessionID, Arg.Any<DateTime>()).Returns(Task.FromResult(true));

		var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);

		Assert.Null(createResult);
		Assert.False(deleteCalled);
		Assert.Equal(sessionID, result);
		await _mockUserSessionRepo.Received().UpdateSession(sessionID, Arg.Any<DateTime>());
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task UserSessionNoCookieButHasOldSessionEndsOldSessionStartsNewOne()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		Action delete = () => { };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.GetSessionIDByUserID(user.UserID).Returns(Task.FromResult(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue }));

		var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);
			
		Assert.Equal(createResult, result);
		await _mockUserSessionRepo.Received().DeleteSessions(user.UserID, sessionID);
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue);
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), user.UserID, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, "1.1.1.1", createResult.ToString(), SecurityLogType.UserSessionStart);
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task UserSessionWithNoMatchingIDEndsOldSessionStartsNewOne()
	{
		var user = new User { UserID = 123 };
		var service = GetService();
		Action delete = () => { };
		int? createResult = null;
		Action<int> create = i => { createResult = i; };
		const int sessionID = 5467;
		_mockUserSessionRepo.GetSessionIDByUserID(user.UserID).Returns(Task.FromResult(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue }));

		var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);
			
		Assert.Equal(createResult, result);
		await _mockUserSessionRepo.Received().UpdateSession(sessionID, Arg.Any<DateTime>());
		await _mockUserSessionRepo.Received().DeleteSessions(user.UserID, sessionID);
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue);
		await _mockUserSessionRepo.Received().CreateSession(Arg.Any<int>(), user.UserID, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user.UserID, Arg.Any<string>(), Arg.Any<string>(), SecurityLogType.UserSessionStart);
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task CleanExpiredSessions()
	{
		var service = GetService();
		var sessions = new List<ExpiredUserSession>
		{
			new ExpiredUserSession { SessionID = 123, UserID = null, LastTime = new DateTime(2000, 2, 5)},
			new ExpiredUserSession { SessionID = 789, UserID = 456, LastTime = new DateTime(2010, 3, 6)}
		};
		_mockUserSessionRepo.GetAndDeleteExpiredSessions(Arg.Any<DateTime>()).Returns(Task.FromResult(sessions));
		_mockSettingsManager.Current.SessionLength.Returns(20);
		await service.CleanUpExpiredSessions();
		await _mockSecurityLogService.Received().CreateLogEntry(null, sessions[0].UserID, String.Empty, sessions[0].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[0].LastTime);
		await _mockSecurityLogService.Received().CreateLogEntry(null, sessions[1].UserID, String.Empty, sessions[1].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[1].LastTime);
	}
}