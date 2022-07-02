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

	public PostImageService(IImageService imageService, IPostImageRepository postImageRepository)
	{
		_imageService = imageService;
		_postImageRepository = postImageRepository;
	}

	private byte[] _bytes;
	private string _contentType;
	private bool _isOk;

	public bool ProcessImageIsOk(byte[] bytes, string contentType)
	{
		_contentType = contentType;
		_bytes = bytes;
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