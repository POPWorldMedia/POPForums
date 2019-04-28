using Moq;
using PopForums.Configuration;
using Xunit;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class ImageServiceTests
	{
		private Mock<IUserImageRepository> _imageRepo;
		private Mock<IUserAvatarRepository> _avatarRepo;
		private Mock<IProfileService> _profileService;
		private Mock<IUserRepository> _userRepo;
		private Mock<ISettingsManager> _settingsManager;

		private ImageService GetService()
		{
			_imageRepo = new Mock<IUserImageRepository>();
			_avatarRepo = new Mock<IUserAvatarRepository>();
			_profileService = new Mock<IProfileService>();
			_userRepo = new Mock<IUserRepository>();
			_settingsManager = new Mock<ISettingsManager>();
			return new ImageService(_avatarRepo.Object, _imageRepo.Object, _profileService.Object, _userRepo.Object, _settingsManager.Object);
		}

		[Fact]
		public void GetAvatar()
		{
			var service = GetService();
			var bytes = new byte[] {};
			_avatarRepo.Setup(a => a.GetImageData(1)).Returns(bytes);

			var result = service.GetAvatarImageData(1);

			Assert.Same(bytes, result);
		}

		[Fact]
		public void GetUserImage()
		{
			var service = GetService();
			var bytes = new byte[] { };
			_imageRepo.Setup(a => a.GetImageData(1)).Returns(bytes);

			var result = service.GetUserImageData(1);

			Assert.Same(bytes, result);
		}
	}
}
