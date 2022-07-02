using Org.BouncyCastle.Utilities;

namespace PopForums.Services;

public interface IPostImageService
{
	bool ProcessImageIsOk(byte[] bytes, string contentType);
	Task<string> PersistAndGetFileName();
}

public class PostImageService : IPostImageService
{
	private readonly IImageService _imageService;
	private readonly IPostImageRepository _postImageRepository;
	private readonly ISettingsManager _settingsManager;

	public PostImageService(IImageService imageService, IPostImageRepository postImageRepository, ISettingsManager settingsManager)
	{
		_imageService = imageService;
		_postImageRepository = postImageRepository;
		_settingsManager = settingsManager;
	}

	private byte[] _bytes;
	private string _contentType;
	private bool _isOk;

	public bool ProcessImageIsOk(byte[] bytes, string contentType)
	{
		_contentType = contentType;
		_bytes = bytes;
		if (bytes.Length > _settingsManager.Current.PostImageMaxkBytes * 1024)
		{
			_isOk = false;
return false;
}
		_bytes = _imageService.ConstrainResize(bytes, _settingsManager.Current.PostImageMaxHeight, _settingsManager.Current.PostImageMaxWidth, 60, false);
		_isOk = true;
		return true;
	}

	public async Task<string> PersistAndGetFileName()
	{
		if (_bytes == null || string.IsNullOrWhiteSpace(_contentType))
			throw new Exception($"No image processed or missing content type. Call {nameof(ProcessImageIsOk)} first.");
		if (!_isOk)
			throw new Exception($"You can't persist an image that was not Ok after calling {nameof(ProcessImageIsOk)}.");
		var fileName = await _postImageRepository.Persist(_bytes, _contentType);
		return fileName;
	}
}