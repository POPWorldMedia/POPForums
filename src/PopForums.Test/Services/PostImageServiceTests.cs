namespace PopForums.Test.Services;

public class PostImageServiceTests
{
	private IImageService _imageService;
	private IPostImageRepository _postImageRepository;
	private IPostImageTempRepository _postImageTempRepository;
	private ISettingsManager _settingsManager;
	private ITenantService _tenantService;

	protected PostImageService GetService()
	{
		_imageService = Substitute.For<IImageService>();
		_postImageRepository = Substitute.For<IPostImageRepository>();
		_postImageTempRepository = Substitute.For<IPostImageTempRepository>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_tenantService = Substitute.For<ITenantService>();
		return new PostImageService(_imageService, _postImageRepository, _postImageTempRepository, _settingsManager, _tenantService);
	}

	public class ProcessImageIsOk : PostImageServiceTests
	{
		[Fact]
		public void ImageIsTooBig()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			var array = new byte[1025];
			var contentType = "blah";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.False(result);
			_imageService.DidNotReceive().ConstrainResize(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), false);
		}

		[Fact]
		public void ImageIsBadContentType()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			var array = new byte[1024];
			var contentType = "blah";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.False(result);
			_imageService.DidNotReceive().ConstrainResize(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), false);
		}

		[Fact]
		public void ImageIsRightSizeJpeg()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			var array = new byte[1024];
			var contentType = "image/jpeg";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.True(result);
		}


		[Fact]
		public void ImageIsRightSizeGif()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
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
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			_settingsManager.Current.PostImageMaxHeight.Returns(height);
			_settingsManager.Current.PostImageMaxWidth.Returns(width);
			var array = new byte[1];
			var contentType = "image/jpeg";

			var result = service.ProcessImageIsOk(array, contentType);

			Assert.True(result);
			_imageService.Received().ConstrainResize(array, width, height, 60, false);
		}
	}

	public class PersistAndGetPayload : PostImageServiceTests
	{
		[Fact]
		public async Task ThrowsWithNoContentType()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			service.ProcessImageIsOk(new byte[1], "");

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async Task ThrowsWhenNotOkContentType()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			service.ProcessImageIsOk(new byte[1], "blah");

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async Task ThrowsWhenNotOkBytes()
		{
			var service = GetService();
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			service.ProcessImageIsOk(new byte[1025], "image/jpeg");

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async void PersistsImageAndTempRecordAndReturnsPayload()
		{
			var service = GetService();
			var tenantID = "pop";
			_tenantService.GetTenant().Returns(tenantID);
			_settingsManager.Current.PostImageMaxkBytes.Returns(1);
			var array = new byte[1];
			_imageService.ConstrainResize(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), false).Returns(array);
			var guid = Guid.NewGuid();
			var id = guid.ToString();
			var payload = new PostImagePersistPayload {ID = id, Url = "neat"};
			_postImageRepository.Persist(Arg.Any<byte[]>(), "image/jpeg").Returns(Task.FromResult(payload));
			service.ProcessImageIsOk(new byte[1], "image/jpeg");

			var result = await service.PersistAndGetPayload();

			await _postImageRepository.Received().Persist(array, "image/jpeg");
			await _postImageTempRepository.Received().Save(guid, Arg.Any<DateTime>(), tenantID);
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

			await _postImageTempRepository.Received().Delete(guid);
		}
	}

	public class DeleteTempRecords : PostImageServiceTests
	{
		[Fact]
		public async void TempRepoCalledWithGuidsFoundInText()
		{
			var service = GetService();
			var guid = Guid.NewGuid();
			var id = guid.ToString();
			var guid2 = Guid.NewGuid();
			var id2 = guid2.ToString();
			var guid3 = Guid.NewGuid();
			var id3 = guid3.ToString();
			var array = new[] {id, id2, id3};
			var text = $"all the words {id3} and ids {id} {id2} ";

			await service.DeleteTempRecords(array, text);

			await _postImageTempRepository.Received().Delete(guid);
			await _postImageTempRepository.Received().Delete(guid2);
		}

		[Fact]
		public async void TempRepoCalledExcludingGuidsNotFoundInText()
		{
			var service = GetService();
			var guid = Guid.NewGuid();
			var id = guid.ToString();
			var guid2 = Guid.NewGuid();
			var id2 = guid2.ToString();
			var guid3 = Guid.NewGuid();
			var id3 = guid3.ToString();
			var array = new[] { id, id2, id3 };
			var text = $"all the words and ids {id} {id3} ";

			await service.DeleteTempRecords(array, text);

			await _postImageTempRepository.Received().Delete(guid);
			await _postImageTempRepository.DidNotReceive().Delete(guid2);
			await _postImageTempRepository.Received().Delete(guid3);
		}
	}

	public class DeleteOldPostImages : PostImageServiceTests
	{
		[Fact]
		public async void PostImageRepoCalledForEachEntry()
		{
			var service = GetService();
			var tenantID = "pop";
			var ids = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()};
			_tenantService.GetTenant().Returns(tenantID);
			_postImageTempRepository.GetOld(Arg.Any<DateTime>()).Returns(Task.FromResult(ids));

			await service.DeleteOldPostImages();

			await _postImageRepository.Received().DeletePostImageData(ids[0].ToString(), tenantID);
			await _postImageRepository.Received().DeletePostImageData(ids[1].ToString(), tenantID);
		}

		[Fact]
		public async void PostImageTempRepoCalledForEachEntry()
		{
			var service = GetService();
			var tenantID = "pop";
			var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
			_tenantService.GetTenant().Returns(tenantID);
			_postImageTempRepository.GetOld(Arg.Any<DateTime>()).Returns(Task.FromResult(ids));

			await service.DeleteOldPostImages();

			await _postImageTempRepository.Received().Delete(ids[0]);
			await _postImageTempRepository.Received().Delete(ids[1]);
		}
	}
}