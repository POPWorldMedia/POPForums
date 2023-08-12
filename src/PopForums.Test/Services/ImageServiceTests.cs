namespace PopForums.Test.Services;

public class ImageServiceTests
{
	private IUserImageRepository _imageRepo;
	private IUserAvatarRepository _avatarRepo;
	private IProfileService _profileService;
	private IUserRepository _userRepo;
	private ISettingsManager _settingsManager;

	private ImageService GetService()
	{
		_imageRepo = Substitute.For<IUserImageRepository>();
		_avatarRepo = Substitute.For<IUserAvatarRepository>();
		_profileService = Substitute.For<IProfileService>();
		_userRepo = Substitute.For<IUserRepository>();
		_settingsManager = Substitute.For<ISettingsManager>();
		return new ImageService(_avatarRepo, _imageRepo, _profileService, _userRepo, _settingsManager);
	}

	[Fact]
	public async Task GetAvatar()
	{
		var service = GetService();
		var bytes = new byte[] {};
		_avatarRepo.GetImageData(1).Returns(Task.FromResult(bytes));

		var result = await service.GetAvatarImageData(1);

		Assert.Same(bytes, result);
	}

	[Fact]
	public async Task GetUserImage()
	{
		var service = GetService();
		var bytes = new byte[] { };
		_imageRepo.GetImageData(1).Returns(Task.FromResult(bytes));

		var result = await service.GetUserImageData(1);

		Assert.Same(bytes, result);
	}
}