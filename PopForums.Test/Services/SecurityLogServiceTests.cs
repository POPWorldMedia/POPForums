using System;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class SecurityLogServiceTests
	{
		private Mock<ISecurityLogRepository> _securityLogRepo;
		private Mock<IUserRepository> _userRepo;

		private SecurityLogService GetService()
		{
			_securityLogRepo = new Mock<ISecurityLogRepository>();
			_userRepo = new Mock<IUserRepository>();
			return new SecurityLogService(_securityLogRepo.Object, _userRepo.Object);
		}

		[Test]
		public void GetEntriesByUserID()
		{
			var service = GetService();
			const int id = 123;
			service.GetLogEntriesByUserID(id, DateTime.MinValue, DateTime.MaxValue);
			_securityLogRepo.Verify(s => s.GetByUserID(id, DateTime.MinValue, DateTime.MaxValue), Times.Once());
		}

		[Test]
		public void GetEntriesByUserName()
		{
			var service = GetService();
			const int id = 123;
			const string name = "jeff";
			_userRepo.Setup(u => u.GetUserByName(name)).Returns(new User(id, DateTime.MinValue) {Name = name});
			service.GetLogEntriesByUserName(name, DateTime.MinValue, DateTime.MaxValue);
			_securityLogRepo.Verify(s => s.GetByUserID(id, DateTime.MinValue, DateTime.MaxValue), Times.Once());
		}

		[Test]
		public void CreateNullIp()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.CreateLogEntry(new User(1, DateTime.MinValue), new User(1, DateTime.MinValue), null, "", SecurityLogType.Undefined));
		}

		[Test]
		public void CreateNullMessage()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.CreateLogEntry(new User(1, DateTime.MinValue), new User(1, DateTime.MinValue), "", null, SecurityLogType.Undefined));
		}

		[Test]
		public void Create()
		{
			var service = GetService();
			SecurityLogEntry entry = null;
			_securityLogRepo.Setup(s => s.Create(It.IsAny<SecurityLogEntry>())).Callback<SecurityLogEntry>(e => entry = e);
			service.CreateLogEntry(new User(1, DateTime.MinValue), new User(2, DateTime.MinValue), "123", "msg", SecurityLogType.Undefined);
			Assert.AreEqual(1, entry.UserID);
			Assert.AreEqual(2, entry.TargetUserID);
			Assert.AreEqual("123", entry.IP);
			Assert.AreEqual("msg", entry.Message);
			Assert.AreEqual(SecurityLogType.Undefined, entry.SecurityLogType);
		}
	}
}
