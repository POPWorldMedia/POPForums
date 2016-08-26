using Moq;
using Xunit;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class ImageServiceTests
	{
		[Fact]
		public void GetAvatar()
		{
			var imageRepoMock = new Mock<IUserImageRepository>();
			var avatarRepoMock = new Mock<IUserAvatarRepository>();
			var profileServiceMock = new Mock<IProfileService>();
			var bytes = new byte[] {};
			avatarRepoMock.Setup(a => a.GetImageData(1)).Returns(bytes);
			var service = new ImageService(avatarRepoMock.Object, imageRepoMock.Object, profileServiceMock.Object);
			var result = service.GetAvatarImageData(1);
			Assert.Same(bytes, result);
		}

		[Fact]
		public void GetUserImage()
		{
			var imageRepoMock = new Mock<IUserImageRepository>();
			var avatarRepoMock = new Mock<IUserAvatarRepository>();
			var profileServiceMock = new Mock<IProfileService>();
			var bytes = new byte[] { };
			imageRepoMock.Setup(a => a.GetImageData(1)).Returns(bytes);
			var service = new ImageService(avatarRepoMock.Object, imageRepoMock.Object, profileServiceMock.Object);
			var result = service.GetUserImageData(1);
			Assert.Same(bytes, result);
		}
	}
}
