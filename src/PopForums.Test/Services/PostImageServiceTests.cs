namespace PopForums.Test.Services;

public class PostImageServiceTests
{
	private Mock<IImageService> _imageService;
	private Mock<IPostImageRepository> _postImageRepository;
	private Mock<IPostImageTempRepository> _postImageTempRepository;
	private Mock<ISettingsManager> _settingsManager;

	protected PostImageService GetService()
	{
		_imageService = new Mock<IImageService>();
		_postImageRepository = new Mock<IPostImageRepository>();
		_postImageTempRepository = new Mock<IPostImageTempRepository>();
		_settingsManager = new Mock<ISettingsManager>();
		return new PostImageService(_imageService.Object, _postImageRepository.Object, _postImageTempRepository.Object, _settingsManager.Object);
	}

	public class ProcessImageIsOk : PostImageServiceTests
	{
		[Fact]
		public void ImageIsTooBig()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			var array = new byte[1025];
			var contentType = "blah";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.False(result);
			_imageService.Verify(x => x.ConstrainResize(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false), Times.Never);
		}

		[Fact]
		public void ImageIsBadContentType()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			var array = new byte[1024];
			var contentType = "blah";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.False(result);
			_imageService.Verify(x => x.ConstrainResize(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false), Times.Never);
		}

		[Fact]
		public void ImageIsRightSizeJpeg()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			var array = new byte[1024];
			var contentType = "image/jpeg";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.True(result);
		}


		[Fact]
		public void ImageIsRightSizeGif()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			var array = new byte[1024];
			var contentType = "image/gif";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.True(result);
		}

		[Fact]
		public void ImageIsResized()
		{
			var height = 100;
			var width = 200;
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			_settingsManager.Setup(x => x.Current.PostImageMaxHeight).Returns(height);
			_settingsManager.Setup(x => x.Current.PostImageMaxWidth).Returns(width);
			var array = new byte[1];
			var contentType = "image/jpeg";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.True(result);
			_imageService.Verify(x => x.ConstrainResize(array, width, height, 60, false), Times.Once);
		}
	}

	public class PersistAndGetPayload : PostImageServiceTests
	{
		[Fact]
		public void ThrowsWithNoContentType()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			service.ProcessImageIsOk(new byte[1], "");

			Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public void ThrowsWhenNotOkContentType()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			service.ProcessImageIsOk(new byte[1], "blah");

			Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public void ThrowsWhenNotOkBytes()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			service.ProcessImageIsOk(new byte[1025], "image/jpeg");

			Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async void PersistsImageAndReturnsPayload()
		{
			var service = GetService();
			_settingsManager.Setup(x => x.Current.PostImageMaxkBytes).Returns(1);
			var array = new byte[1];
			_imageService.Setup(x => x.ConstrainResize(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false)).Returns(array);
			var id = Guid.NewGuid().ToString();
			var payload = new PostImagePersistPayload {ID = id, Url = "neat"};
			_postImageRepository.Setup(x => x.Persist(It.IsAny<byte[]>(), "image/jpeg")).ReturnsAsync(payload);
			service.ProcessImageIsOk(new byte[1], "image/jpeg");

			var result = await service.PersistAndGetPayload();

			_postImageRepository.Verify(x => x.Persist(array, "image/jpeg"), Times.Once);
			Assert.Same(payload, result);
		}
	}

	public class DeleteTempRecord : PostImageServiceTests
	{
		[Fact]
		public async void TempRepoCalledWithGuid()
		{
			var service = GetService();
			var guid = Guid.NewGuid();
			var id = guid.ToString();

			await service.DeleteTempRecord(id);

			_postImageTempRepository.Verify(x => x.Delete(guid), Times.Once);
		}
	}

	public class DeleteTempRecords : PostImageServiceTests
	{
		[Fact]
		public async void TempRepoCalledWithGuid()
		{
			var service = GetService();
			var guid = Guid.NewGuid();
			var id = guid.ToString();
			var guid2 = Guid.NewGuid();
			var id2 = guid2.ToString();
			var array = new[] {id, id2};

			await service.DeleteTempRecords(array);

			_postImageTempRepository.Verify(x => x.Delete(guid), Times.Once);
			_postImageTempRepository.Verify(x => x.Delete(guid2), Times.Once);
		}
	}
}