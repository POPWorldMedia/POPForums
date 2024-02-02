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
		var streamResponse = Substitute.For<IStreamResponse>();
		_avatarRepo.GetImageStream(1).Returns(Task.FromResult(streamResponse));

		var result = await service.GetAvatarImageStream(1);

		Assert.Same(streamResponse, result);
	}

	[Fact]
	public async Task GetUserImage()
	{
		var service = GetService();
		var streamResponse = Substitute.For<IStreamResponse>();
		_imageRepo.GetImageStream(1).Returns(Task.FromResult(streamResponse));

		var result = await service.GetUserImageStream(1);

		Assert.Same(streamResponse, result);
	}
}