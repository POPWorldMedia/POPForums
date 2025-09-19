namespace PopForums.Repositories;

public interface IIgnoreRepository
{
	Task AddIgnore(int userID, int ignoreUserID);
	Task DeleteIgnore(int userID, int ignoreUserID);
	Task<IList<IgnoreWithName>> GetIgnoreList(int userID);
	Task<List<int>> GetIgnoredUserIdsInList(int userID, List<int> userIDs);
}