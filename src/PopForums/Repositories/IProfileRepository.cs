using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IProfileRepository
	{
		Task<Profile> GetProfile(int userID);
		Task Create(Profile profile);
		Task<bool> Update(Profile profile);
		Task<int?> GetLastPostID(int userID);
		Task<bool> SetLastPostID(int userID, int postID);
		Task<Dictionary<int, string>> GetSignatures(List<int> userIDs);
		Task<Dictionary<int, int>> GetAvatars(List<int> userIDs);
		Task SetCurrentImageIDToNull(int userID);
		Task UpdatePoints(int userID, int points);
	}
}