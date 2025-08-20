namespace PopForums.Services;

public interface IIgnoreService
{
	Task<IList<IgnoreWithName>> GetIgnoreList(int userID);
	Task AddIgnore(int userID, int ignoreUserID);
	Task DeleteIgnore(int userID, int ignoreUserID);
	Task<List<int>> GetIgnoredUserIdsInList(User user, List<Post> posts);
}

public class IgnoreService(IIgnoreRepository ignoreRepository) : IIgnoreService
{
	public async Task<IList<IgnoreWithName>> GetIgnoreList(int userID)
	{
		return await ignoreRepository.GetIgnoreList(userID);
	}
	
	public async Task AddIgnore(int userID, int ignoreUserID)
	{
		await ignoreRepository.AddIgnore(userID, ignoreUserID);
	}
	
	public async Task DeleteIgnore(int userID, int ignoreUserID)
	{
		await ignoreRepository.DeleteIgnore(userID, ignoreUserID);
	}
	
	public async Task<List<int>> GetIgnoredUserIdsInList(User user, List<Post> posts)
	{
		if (user == null)
			return new List<int>();
		var userIDs = posts.Select(p => p.UserID).Distinct().ToList();
		return await ignoreRepository.GetIgnoredUserIdsInList(user.UserID, userIDs);
	}
}