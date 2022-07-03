namespace PopForums.Repositories;

public interface IPostImageTempRepository
{
	Task Save(Guid postImageTempID, DateTime timeStamp, string tenantID);
	Task Delete(Guid id);
	Task<List<Guid>> GetOld(DateTime olderThan);
}