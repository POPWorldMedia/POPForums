namespace PopForums.Repositories;

public interface IPostImageRepository
{
	Task<string> Persist(byte[] bytes, string contentType);
	Task<PostImage> GetWithoutData(string id);
	Task<PostImage> Get(string id);
}