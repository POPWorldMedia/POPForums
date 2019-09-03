using System;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Test.Services
{
	public class UserSessionServiceTests
	{
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<IUserRepository> _mockUserRepo;
		private Mock<IUserSessionRepository> _mockUserSessionRepo;
		private Mock<ISecurityLogService> _mockSecurityLogService;

		private UserSessionService GetService()
		{
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockUserRepo = new Mock<IUserRepository>();
			_mockUserSessionRepo = new Mock<IUserSessionRepository>();
			_mockSecurityLogService = new Mock<ISecurityLogService>();
			var service = new UserSessionService(_mockSettingsManager.Object, _mockUserRepo.Object, _mockUserSessionRepo.Object, _mockSecurityLogService.Object);
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
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), null, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((int?)null, null, "1.1.1.1", createResult.Value.ToString(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never());
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
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).ReturnsAsync(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).ReturnsAsync(true);

			var result = await service.ProcessUserRequest(null, sessionID, "1.1.1.1", delete, create);

			Assert.False(deleteCalled);
			Assert.Equal(sessionID, result);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never());
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
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).ReturnsAsync(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).ReturnsAsync(true);

			var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);

			Assert.True(deleteCalled);
			Assert.Equal(createResult, result);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(null, sessionID), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).ReturnsAsync(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).ReturnsAsync(false);

			var result = await service.ProcessUserRequest(null, sessionID, "1.1.1.1", delete, create);

			Assert.True(deleteCalled);
			Assert.Equal(createResult, result);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(null, sessionID), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), null, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((int?)null, null, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(It.IsAny<int>())).ReturnsAsync((ExpiredUserSession)null);

			var result = await service.ProcessUserRequest(user, null, "1.1.1.1", delete, create);

			Assert.False(deleteCalled);
			Assert.Equal(createResult, result);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), 123, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), result.ToString(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).ReturnsAsync(true);

			var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);

			Assert.Null(createResult);
			Assert.False(deleteCalled);
			Assert.Equal(sessionID, result);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(user.UserID)).ReturnsAsync(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue });

			var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);
			
			Assert.Equal(createResult, result);
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(user.UserID, sessionID));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue), Times.Once());
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, "1.1.1.1", createResult.ToString(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(user.UserID)).ReturnsAsync(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue });

			var result = await service.ProcessUserRequest(user, sessionID, "1.1.1.1", delete, create);
			
			Assert.Equal(createResult, result);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(user.UserID, sessionID));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue), Times.Once());
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
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
			_mockUserSessionRepo.Setup(u => u.GetAndDeleteExpiredSessions(It.IsAny<DateTime>())).ReturnsAsync(sessions);
			_mockSettingsManager.Setup(s => s.Current.SessionLength).Returns(20);
			await service.CleanUpExpiredSessions();
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, sessions[0].UserID, String.Empty, sessions[0].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[0].LastTime), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, sessions[1].UserID, String.Empty, sessions[1].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[1].LastTime), Times.Once());
		}
	}
}