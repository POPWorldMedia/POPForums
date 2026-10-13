namespace PopForums.Test.Services;

public class PostImageServiceTests
{
	private IPostImageRepository _postImageRepository;
	private IPostImageTempRepository _postImageTempRepository;
	private ISettingsManager _settingsManager;
	private ITenantService _tenantService;
	private IImageInspector _imageInspector;

	protected PostImageService GetService()
	{
		_postImageRepository = Substitute.For<IPostImageRepository>();
		_postImageTempRepository = Substitute.For<IPostImageTempRepository>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_tenantService = Substitute.For<ITenantService>();
		_imageInspector = Substitute.For<IImageInspector>();
		_settingsManager.Current.PostImageMaxWidth.Returns(200);
		_settingsManager.Current.PostImageMaxHeight.Returns(100);
		_settingsManager.Current.PostImageMaxkBytes.Returns(1);
		return new PostImageService(_postImageRepository, _postImageTempRepository, _settingsManager, _tenantService, _imageInspector);
	}

	protected void SetupInspector(ImageValidationResult result, ImageInfo info)
	{
		_imageInspector.Validate(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), out Arg.Any<ImageInfo>())
			.Returns(x =>
			{
				x[4] = info;
				return result;
			});
	}

	public class ProcessImageIsOk : PostImageServiceTests
	{
		[Fact]
		public void ValidatesWithPostImageLimits()
		{
			var service = GetService();
			SetupInspector(ImageValidationResult.Valid, new ImageInfo(ImageFormat.Jpeg, 1, 1));
			var array = new byte[1];

			service.ProcessImageIsOk(array);

			_imageInspector.Received().Validate(array, 200, 100, 1, out Arg.Any<ImageInfo>());
		}

		[Theory]
		[InlineData(ImageValidationResult.Invalid)]
		[InlineData(ImageValidationResult.TooLarge)]
		public void FalseWhenNotValid(ImageValidationResult validationResult)
		{
			var service = GetService();
			SetupInspector(validationResult, null);

			var result = service.ProcessImageIsOk(new byte[1]);

			Assert.False(result);
		}

		[Fact]
		public void TrueWhenValid()
		{
			var service = GetService();
			SetupInspector(ImageValidationResult.Valid, new ImageInfo(ImageFormat.Gif, 1, 1));

			var result = service.ProcessImageIsOk(new byte[1]);

			Assert.True(result);
		}
	}

	public class PersistAndGetPayload : PostImageServiceTests
	{
		[Fact]
		public async Task ThrowsWithoutProcessing()
		{
			var service = GetService();

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async Task ThrowsWhenInvalid()
		{
			var service = GetService();
			SetupInspector(ImageValidationResult.Invalid, null);
			service.ProcessImageIsOk(new byte[1]);

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async Task ThrowsWhenTooLarge()
		{
			var service = GetService();
			SetupInspector(ImageValidationResult.TooLarge, new ImageInfo(ImageFormat.Jpeg, 1, 1));
			service.ProcessImageIsOk(new byte[1]);

			await Assert.ThrowsAsync<Exception>(() => service.PersistAndGetPayload());
		}

		[Fact]
		public async Task PersistsImageAndTempRecordAndReturnsPayload()
		{
			var service = GetService();
			var tenantID = "pop";
			_tenantService.GetTenant().Returns(tenantID);
			SetupInspector(ImageValidationResult.Valid, new ImageInfo(ImageFormat.Jpeg, 1, 1));
			var array = new byte[1];
			var guid = Guid.NewGuid();
			var id = guid.ToString();
			var payload = new PostImagePersistPayload {ID = id, Url = "neat"};
			_postImageRepository.Persist(Arg.Any<byte[]>(), "image/jpeg").Returns(Task.FromResult(payload));
			service.ProcessImageIsOk(array);

			var result = await service.PersistAndGetPayload();

			await _postImageRepository.Received().Persist(array, "image/jpeg");
			await _postImageTempRepository.Received().Save(guid, Arg.Any<DateTime>(), tenantID);
			Assert.Same(payload, result);
		}

		[Fact]
		public async Task PersistsContentTypeFromInspector()
		{
			var service = GetService();
			_tenantService.GetTenant().Returns("pop");
			SetupInspector(ImageValidationResult.Valid, new ImageInfo(ImageFormat.Png, 1, 1));
			var array = new byte[1];
			_postImageRepository.Persist(Arg.Any<byte[]>(), Arg.Any<string>()).Returns(Task.FromResult(new PostImagePersistPayload { ID = Guid.NewGuid().ToString() }));
			service.ProcessImageIsOk(array);

			await service.PersistAndGetPayload();

			await _postImageRepository.Received().Persist(array, "image/png");
		}
	}

	public class DeleteTempRecord : PostImageServiceTests
	{
		[Fact]
		public async Task TempRepoCalledWithGuid()
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
		public async Task TempRepoCalledWithGuidsFoundInText()
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
		public async Task TempRepoCalledExcludingGuidsNotFoundInText()
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
		public async Task PostImageRepoCalledForEachEntry()
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
		public async Task PostImageTempRepoCalledForEachEntry()
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