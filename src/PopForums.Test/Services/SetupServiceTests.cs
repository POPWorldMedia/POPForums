namespace PopForums.Test.Services;

public class SetupServiceTests
{
	private Mock<ISetupRepository> _setupRepository;
	private Mock<IUserService> _userService;
	private Mock<ISettingsManager> _settingsManager;
	private Mock<IProfileService> _profileService;

	private SetupService GetService()
	{
		// kind of gross leaky abstraction here
		var staticSetupIndicator = typeof(SetupService).GetField("_isConnectionSetupGood", BindingFlags.Static | BindingFlags.NonPublic);
		staticSetupIndicator.SetValue(null, null);
		_setupRepository = new Mock<ISetupRepository>();
		_userService = new Mock<IUserService>();
		_settingsManager = new Mock<ISettingsManager>();
		_profileService = new Mock<IProfileService>();
		return new SetupService(_setupRepository.Object, _userService.Object, _settingsManager.Object,
			_profileService.Object);
	}

	public class IsRuntimeConnectionAndSetupGoodTests : SetupServiceTests
	{
		[Fact]
		public void GoodConnectionAndSetupAlwaysReturnsTrue()
		{
			var service = GetService();
			_setupRepository.Setup(x => x.IsConnectionPossible()).Returns(true);
			_setupRepository.Setup(x => x.IsDatabaseSetup()).Returns(true);

			var result1 = service.IsRuntimeConnectionAndSetupGood();
			var result2 = service.IsRuntimeConnectionAndSetupGood();

			Assert.True(result1);
			Assert.True(result2);
		}

		[Fact]
		public void GoodConnectionAndSetupOnlyCallsReposOnce()
		{
			var service = GetService();
			_setupRepository.Setup(x => x.IsConnectionPossible()).Returns(true);
			_setupRepository.Setup(x => x.IsDatabaseSetup()).Returns(true);

			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();
			service.IsRuntimeConnectionAndSetupGood();

			_setupRepository.Verify(x => x.IsDatabaseSetup(), Times.Once);
			_setupRepository.Verify(x => x.IsConnectionPossible(), Times.Once);
		}

		[Fact]
		public void BadConnectionReturnsFalse()
		{
			var service = GetService();
			_setupRepository.Setup(x => x.IsConnectionPossible()).Returns(false);

			var result = service.IsRuntimeConnectionAndSetupGood();

			Assert.False(result);
		}

		[Fact]
		public void GoodConnectionButNotSetupReturnsFalse()
		{
			var service = GetService();
			_setupRepository.Setup(x => x.IsConnectionPossible()).Returns(true);
			_setupRepository.Setup(x => x.IsDatabaseSetup()).Returns(false);

			var result = service.IsRuntimeConnectionAndSetupGood();

			Assert.False(result);
		}
	}
}