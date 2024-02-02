namespace PopForums.Repositories;

public interface IPostImageRepository
{
	Task<PostImagePersistPayload> Persist(byte[] bytes, string contentType);
	Task<PostImage> GetWithoutData(string id);
	[Obsolete("Use the combination of GetWithoutData(int) and GetImageStream(int) instead.")]
	Task<PostImage> Get(string id);
	Task DeletePostImageData(string id, string tenantID);
	Task<IStreamResponse> GetImageStream(string id);
}