namespace PopForums.Test.Services;

public class SecurityLogServiceTests
{
	private ISecurityLogRepository _securityLogRepo;
	private IUserRepository _userRepo;

	private SecurityLogService GetService()
	{
		_securityLogRepo = Substitute.For<ISecurityLogRepository>();
		_userRepo = Substitute.For<IUserRepository>();
		return new SecurityLogService(_securityLogRepo, _userRepo);
	}

	[Fact]
	public async Task GetEntriesByUserID()
	{
		var service = GetService();
		const int id = 123;
		await service.GetLogEntriesByUserID(id, DateTime.MinValue, DateTime.MaxValue);
		await _securityLogRepo.Received().GetByUserID(id, DateTime.MinValue, DateTime.MaxValue);
	}

	[Fact]
	public async Task GetEntriesByUserName()
	{
		var service = GetService();
		const int id = 123;
		const string name = "jeff";
		_userRepo.GetUserByName(name).Returns(Task.FromResult(new User { UserID = id, Name = name}));
		await service.GetLogEntriesByUserName(name, DateTime.MinValue, DateTime.MaxValue);
		await _securityLogRepo.Received().GetByUserID(id, DateTime.MinValue, DateTime.MaxValue);
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
		await _securityLogRepo.Create(Arg.Do<SecurityLogEntry>(x => entry = x));
		await service.CreateLogEntry(new User { UserID = 1 }, new User { UserID = 2 }, "123", "msg", SecurityLogType.Undefined);
		Assert.Equal(1, entry.UserID);
		Assert.Equal(2, entry.TargetUserID);
		Assert.Equal("123", entry.IP);
		Assert.Equal("msg", entry.Message);
		Assert.Equal(SecurityLogType.Undefined, entry.SecurityLogType);
	}
}