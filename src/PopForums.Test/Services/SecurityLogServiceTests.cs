using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
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

		[Fact]
		public async Task GetEntriesByUserID()
		{
			var service = GetService();
			const int id = 123;
			await service.GetLogEntriesByUserID(id, DateTime.MinValue, DateTime.MaxValue);
			_securityLogRepo.Verify(s => s.GetByUserID(id, DateTime.MinValue, DateTime.MaxValue), Times.Once());
		}

		[Fact]
		public async Task GetEntriesByUserName()
		{
			var service = GetService();
			const int id = 123;
			const string name = "jeff";
			_userRepo.Setup(u => u.GetUserByName(name)).ReturnsAsync(new User { UserID = id, Name = name});
			await service.GetLogEntriesByUserName(name, DateTime.MinValue, DateTime.MaxValue);
			_securityLogRepo.Verify(s => s.GetByUserID(id, DateTime.MinValue, DateTime.MaxValue), Times.Once());
		}

		[Fact]
		public async Task CreateNullIp()
		{
			var service = GetService();
			await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateLogEntry(new User(), new User(), null, "", SecurityLogType.Undefined));
		}

		[Fact]
		public async Task CreateNullMessage()
		{
			var service = GetService();
			await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateLogEntry(new User(), new User(), "", null, SecurityLogType.Undefined));
		}

		[Fact]
		public async Task Create()
		{
			var service = GetService();
			SecurityLogEntry entry = null;
			_securityLogRepo.Setup(s => s.Create(It.IsAny<SecurityLogEntry>())).Callback<SecurityLogEntry>(e => entry = e);
			await service.CreateLogEntry(new User { UserID = 1 }, new User { UserID = 2 }, "123", "msg", SecurityLogType.Undefined);
			Assert.Equal(1, entry.UserID);
			Assert.Equal(2, entry.TargetUserID);
			Assert.Equal("123", entry.IP);
			Assert.Equal("msg", entry.Message);
			Assert.Equal(SecurityLogType.Undefined, entry.SecurityLogType);
		}
	}
}
