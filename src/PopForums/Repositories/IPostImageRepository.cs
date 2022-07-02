namespace PopForums.Repositories;

public interface IPostImageRepository
{
	Task<string> Persist(byte[] bytes, string contentType);
}