using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFriendRepository
	{
		Task<List<Friend>> GetFriends(int userID);
		Task<IEnumerable<Friend>> GetFriendsOf(int userID);
		Task<List<User>> GetUnapprovedFriends(int userID);
		Task AddUnapprovedFriend(int fromUserID, int toUserID);
		Task DeleteFriend(int fromUserID, int toUserID);
		Task ApproveFriend(int fromUserID, int toUserID);
		Task<bool> IsFriend(int fromUserID, int toUserID);
	}
}