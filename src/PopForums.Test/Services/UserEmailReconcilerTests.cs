namespace PopForums.Test.Services;

public class UserEmailReconcilerTests
{
	private IUserRepository _userRepo;

	private UserEmailReconciler GetService()
	{
		_userRepo = Substitute.For<IUserRepository>();
		return new UserEmailReconciler(_userRepo);
	}

	public class GetUniqueEmail : UserEmailReconcilerTests
	{
		[Fact]
		public async Task UnmatchedReturnsSameEmail()
		{
			var service = GetService();
			var email = "a@b.com";
			_userRepo.GetUserByEmail(email).Returns((User)null);

			var result = await service.GetUniqueEmail(email, "12345");
			
			Assert.Equal(email, result);
		}
		
		[Fact]
		public async Task MatchedReturnsUniqueEmail()
		{
			var service = GetService();
			var email = "a@b.com";
			var user = new User { Email = email };
			_userRepo.GetUserByEmail(email).Returns(Task.FromResult(user));

			var result = await service.GetUniqueEmail(email, "12345");
			
			Assert.Equal("a-at-b.com@12345.example.com", result);
		}
	}
}