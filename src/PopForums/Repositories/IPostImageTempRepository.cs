namespace PopForums.Repositories;

public interface IPostImageTempRepository
{
	Task Save(Guid postImageTempID, DateTime timeStamp);
	Task Delete(Guid id);
	Task Purge(DateTime olderThan);
}