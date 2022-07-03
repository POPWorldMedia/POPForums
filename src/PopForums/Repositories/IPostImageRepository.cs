namespace PopForums.Repositories;

public interface IPostImageRepository
{
	Task<PostImagePersistPayload> Persist(byte[] bytes, string contentType);
	Task<PostImage> GetWithoutData(string id);
	Task<PostImage> Get(string id);
}