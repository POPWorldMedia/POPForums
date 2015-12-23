using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IProfileRepository
	{
		Profile GetProfile(int userID);
		void Create(Profile profile);
		bool Update(Profile profile);
		int? GetLastPostID(int userID);
		bool SetLastPostID(int userID, int postID);
		Dictionary<int, string> GetSignatures(List<int> userIDs);
		Dictionary<int, int> GetAvatars(List<int> userIDs);
		void SetCurrentImageIDToNull(int userID);
		void UpdatePoints(int userID, int points);
	}
}