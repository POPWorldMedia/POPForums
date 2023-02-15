namespace PopForums.Test.Services;

public class UserNameReconcilerTests
{
	private Mock<IUserRepository> _userRepo;

	private UserNameReconciler GetService()
	{
		_userRepo = new Mock<IUserRepository>();
		return new UserNameReconciler(_userRepo.Object);
	}

	public class GetUniqueNameForNewUser : UserNameReconcilerTests
	{
		[Fact]
		public async Task NoMatchesReturnsSameName()
		{
			var name = "Jeff P";
			var service = GetService();
			_userRepo.Setup(x => x.GetUserNamesThatStartWith(name)).ReturnsAsync(new string[]{});

			var result = await service.GetUniqueNameForUser(name);
			
			Assert.Equal(name, result);
		}
		
		[Fact]
		public async Task OneMatchesReturnsAppendedName()
		{
			var name = "Jeff P";
			var service = GetService();
			_userRepo.Setup(x => x.GetUserNamesThatStartWith(name)).ReturnsAsync(new []{ name });

			var result = await service.GetUniqueNameForUser(name);
			
			Assert.Equal("Jeff P-2", result);
		}
		
		[Fact]
		public async Task ThreeMatchesReturnsAppendedName()
		{
			var name = "Jeff P";
			var service = GetService();
			_userRepo.Setup(x => x.GetUserNamesThatStartWith(name)).ReturnsAsync(new []{ "Jeff P", "Jeff P-2", "Jeff P-3" });

			var result = await service.GetUniqueNameForUser(name);
			
			Assert.Equal("Jeff P-4", result);
		}
		
		[Fact]
		public async Task ThreeMatchesWithOneExtraReturnsAppendedName()
		{
			var name = "Jeff P";
			var service = GetService();
			_userRepo.Setup(x => x.GetUserNamesThatStartWith(name)).ReturnsAsync(new []{ "Jeff P", "Jeff Peterson", "Jeff P-2" });

			var result = await service.GetUniqueNameForUser(name);
			
			Assert.Equal("Jeff P-3", result);
		}
	}
}