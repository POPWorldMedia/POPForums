using System.Net.Mime;

namespace PopForums.Services;

public interface IPostImageService
{
	bool ProcessImageIsOk(byte[] bytes, string contentType);
	Task<PostImagePersistPayload> PersistAndGetPayload();
	Task<PostImage> GetWithoutData(string id);
	Task<PostImage> Get(string id);
	Task DeleteTempRecord(string id);
	Task DeleteTempRecords(string[] ids);
}

public class PostImageService : IPostImageService
{
	private readonly IImageService _imageService;
	private readonly IPostImageRepository _postImageRepository;
	private readonly IPostImageTempRepository _postImageTempRepository;
	private readonly ISettingsManager _settingsManager;
	private readonly ITenantService _tenantService;

	public PostImageService(IImageService imageService, IPostImageRepository postImageRepository, IPostImageTempRepository postImageTempRepository, ISettingsManager settingsManager, ITenantService tenantService)
	{
		_imageService = imageService;
		_postImageRepository = postImageRepository;
		_postImageTempRepository = postImageTempRepository;
		_settingsManager = settingsManager;
		_tenantService = tenantService;
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

		if (contentType != MediaTypeNames.Image.Jpeg && contentType != MediaTypeNames.Image.Gif)
		{
			_isOk = false;
			return false;
		}

		_bytes = _imageService.ConstrainResize(bytes, _settingsManager.Current.PostImageMaxWidth, _settingsManager.Current.PostImageMaxHeight, 60, false);
		_isOk = true;
		return true;
	}

	public async Task<PostImagePersistPayload> PersistAndGetPayload()
	{
		if (_bytes == null || string.IsNullOrWhiteSpace(_contentType))
			throw new Exception($"No image processed or missing content type. Call {nameof(ProcessImageIsOk)} first.");
		if (!_isOk)
			throw new Exception($"You can't persist an image that was not Ok after calling {nameof(ProcessImageIsOk)}.");
		var payload = await _postImageRepository.Persist(_bytes, _contentType);
		var tenantID = _tenantService.GetTenant();
		await _postImageTempRepository.Save(Guid.Parse(payload.ID), DateTime.UtcNow, tenantID);
		return payload;
	}

	public async Task<PostImage> GetWithoutData(string id)
	{
		var postImageSansData = await _postImageRepository.GetWithoutData(id);
		return postImageSansData;
	}

	public async Task<PostImage> Get(string id)
	{
		var postImageSansData = await _postImageRepository.Get(id);
		return postImageSansData;
	}

	public async Task DeleteTempRecord(string id)
	{
		var guid = Guid.Parse(id);
		await _postImageTempRepository.Delete(guid);
	}

	public async Task DeleteTempRecords(string[] ids)
	{
		foreach (var id in ids)
			await DeleteTempRecord(id);
	}

}