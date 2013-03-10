using System;
using System.Web;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Test.Controllers;
using System.Collections.Generic;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class UserSessionServiceTests
	{
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<IUserRepository> _mockUserRepo;
		private Mock<IUserSessionRepository> _mockUserSessionRepo;
		private Mock<ISecurityLogService> _mockSecurityLogService;
		private HttpContextHelper _contextHelper;

		private UserSessionService GetService()
		{
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockUserRepo = new Mock<IUserRepository>();
			_mockUserSessionRepo = new Mock<IUserSessionRepository>();
			_mockSecurityLogService = new Mock<ISecurityLogService>();
			_contextHelper = new HttpContextHelper();
			var service = new UserSessionService(_mockSettingsManager.Object, _mockUserRepo.Object, _mockUserSessionRepo.Object, _mockSecurityLogService.Object);
			return service;
		}

		[Test]
		public void AnonUserNoCookieGetsCookieAndSessionStart()
		{
			var service = GetService();
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(new HttpCookieCollection());
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);
			service.ProcessUserRequest(null, _contextHelper.MockContext.Object);
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int sessionID;
			Assert.True(int.TryParse(cookie.Value, out sessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), null, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((int?)null, null, It.IsAny<string>(), sessionID.ToString(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never());
		}

		[Test]
		public void AnonUserWithCookieUpdateSession()
		{
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection {new HttpCookie("pf_sessionID", sessionID.ToString())};
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).Returns(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).Returns(true);
			service.ProcessUserRequest(null, _contextHelper.MockContext.Object);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never());
		}

		[Test]
		public void UserWithAnonCookieStartsLoggedInSession()
		{
			var user = new User(123, DateTime.MinValue);
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection { new HttpCookie("pf_sessionID", sessionID.ToString()) };
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).Returns(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).Returns(true);
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);

			service.ProcessUserRequest(user, _contextHelper.MockContext.Object);

			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(null, sessionID), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, It.IsAny<DateTime>()), Times.Once());
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int newSessionID;
			Assert.True(int.TryParse(cookie.Value, out newSessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
		}

		[Test]
		public void AnonUserWithLoggedInSessionEndsOldOneStartsNewOne()
		{
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection { new HttpCookie("pf_sessionID", sessionID.ToString()) };
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).Returns(true);
			_mockUserSessionRepo.Setup(u => u.IsSessionAnonymous(sessionID)).Returns(false);
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);

			service.ProcessUserRequest(null, _contextHelper.MockContext.Object);

			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(null, sessionID), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, null, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, It.IsAny<DateTime>()), Times.Once());
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int newSessionID;
			Assert.True(int.TryParse(cookie.Value, out newSessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), null, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((int?)null, null, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
		}

		[Test]
		public void UserWithNoCookieGetsCookieAndSessionStart()
		{
			var user = new User(123, DateTime.MinValue);
			var service = GetService();
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(new HttpCookieCollection());
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(It.IsAny<int>())).Returns((ExpiredUserSession)null);
			service.ProcessUserRequest(user, _contextHelper.MockContext.Object);
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int sessionID;
			Assert.True(int.TryParse(cookie.Value, out sessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), 123, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), sessionID.ToString(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void UserWithCookieUpdateSession()
		{
			var user = new User(123, DateTime.MinValue);
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection { new HttpCookie("pf_sessionID", sessionID.ToString()) };
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).Returns(true);
			service.ProcessUserRequest(user, _contextHelper.MockContext.Object);
			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void UserSessionNoCookieButHasOldSessionEndsOldSessionStartsNewOne()
		{
			var user = new User(123, DateTime.MinValue);
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection();
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(user.UserID)).Returns(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue });

			service.ProcessUserRequest(user, _contextHelper.MockContext.Object);

			_mockUserSessionRepo.Verify(u => u.DeleteSessions(user.UserID, sessionID));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue), Times.Once());
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int newSessionID;
			Assert.True(int.TryParse(cookie.Value, out newSessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void UserSessionWithNoMatchingIDEndsOldSessionStartsNewOne()
		{
			var user = new User(123, DateTime.MinValue);
			var service = GetService();
			const int sessionID = 5467;
			var requestCookies = new HttpCookieCollection { new HttpCookie("pf_sessionID", sessionID.ToString()) };
			_contextHelper.MockRequest.Setup(r => r.Cookies).Returns(requestCookies);
			_mockUserSessionRepo.Setup(u => u.UpdateSession(sessionID, It.IsAny<DateTime>())).Returns(false);
			var responseCookies = new HttpCookieCollection();
			_contextHelper.MockResponse.Setup(r => r.Cookies).Returns(responseCookies);
			_mockUserSessionRepo.Setup(u => u.GetSessionIDByUserID(user.UserID)).Returns(new ExpiredUserSession { UserID = user.UserID, SessionID = sessionID, LastTime = DateTime.MinValue });

			service.ProcessUserRequest(user, _contextHelper.MockContext.Object);

			_mockUserSessionRepo.Verify(u => u.UpdateSession(sessionID, It.IsAny<DateTime>()), Times.Once());
			_mockUserSessionRepo.Verify(u => u.DeleteSessions(user.UserID, sessionID));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, String.Empty, sessionID.ToString(), SecurityLogType.UserSessionEnd, DateTime.MinValue), Times.Once());
			Assert.AreEqual(1, responseCookies.Count);
			var cookie = responseCookies[0];
			int newSessionID;
			Assert.True(int.TryParse(cookie.Value, out newSessionID));
			Assert.AreEqual("pf_sessionID", cookie.Name);
			_mockUserSessionRepo.Verify(u => u.CreateSession(It.IsAny<int>(), user.UserID, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user.UserID, It.IsAny<string>(), It.IsAny<string>(), SecurityLogType.UserSessionStart), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void CleanExpiredSessions()
		{
			var service = GetService();
			var sessions = new List<ExpiredUserSession>
			               	{
			               		new ExpiredUserSession { SessionID = 123, UserID = null, LastTime = new DateTime(2000, 2, 5)},
			               		new ExpiredUserSession { SessionID = 789, UserID = 456, LastTime = new DateTime(2010, 3, 6)}
			               	};
			_mockUserSessionRepo.Setup(u => u.GetAndDeleteExpiredSessions(It.IsAny<DateTime>())).Returns(sessions);
			_mockSettingsManager.Setup(s => s.Current.SessionLength).Returns(20);
			service.CleanUpExpiredSessions();
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, sessions[0].UserID, String.Empty, sessions[0].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[0].LastTime), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, sessions[1].UserID, String.Empty, sessions[1].SessionID.ToString(), SecurityLogType.UserSessionEnd, sessions[1].LastTime), Times.Once());
		}
	}
}