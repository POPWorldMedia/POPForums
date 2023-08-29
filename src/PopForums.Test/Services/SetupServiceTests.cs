namespace PopForums.Test.Services;

public class SetupServiceTests
{
	private ISetupRepository _setupRepository;
	private IUserService _userService;
	private ISettingsManager _settingsManager;
	private IProfileService _profileService;

	private SetupService GetService()
	{
		// kind of gross leaky abstraction here
		var staticSetupIndicator = typeof(SetupService).GetField("_isConnectionSetupGood", BindingFlags.Static | BindingFlags.NonPublic);
		staticSetupIndicator.SetValue(null, null);
		_setupRepository = Substitute.For<ISetupRepository>();
		_userService = Substitute.For<IUserService>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_profileService = Substitute.For<IProfileService>();
		return new SetupService(_setupRepository, _userService, _settingsManager,
			_profileService);
	}

	public class IsRuntimeConnectionAndSetupGoodTests : SetupServiceTests
	{
		[Fact]
		public void GoodConnectionAndSetupAlwaysReturnsTrue()
		{
			var service = GetService();
			_setupRepository.IsConnectionPossible().Returns(true);
			_setupRepository.IsDatabaseSetup().Returns(true);

			var result1 = service.IsRuntimeConnectionAndSetupGood();
			var result2 = service.IsRuntimeConnectionAndSetupGood();

			Assert.True(result1);
			Assert.True(result2);
		}

		[Fact]
		public void GoodConnectionAndSetupOnlyCallsReposOnce()
		{
			var service = GetService();
			_setupRepository.IsConnectionPossible().Returns(true);
			_setupRepository.IsDatabaseSetup().Returns(true);

			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();

			_setupRepository.Received().IsDatabaseSetup();
			_setupRepository.Received().IsConnectionPossible();
		}

		[Fact]
		public void BadConnectionReturnsFalse()
		{
			var service = GetService();
			_setupRepository.IsConnectionPossible().Returns(false);

			var result = service.IsRuntimeConnectionAndSetupGood();

			Assert.False(result);
		}

		[Fact]
		public void GoodConnectionButNotSetupReturnsFalse()
		{
			var service = GetService();
			_setupRepository.IsConnectionPossible().Returns(true);
			_setupRepository.IsDatabaseSetup().Returns(false);

			var result = service.IsRuntimeConnectionAndSetupGood();

			Assert.False(result);
		}
	}
}