using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFriendRepository
	{
		List<Friend> GetFriends(int userID);
		List<Friend> GetFriendsOf(int userID);
		List<User> GetUnapprovedFriends(int userID);
		void AddUnapprovedFriend(int fromUserID, int toUserID);
		void DeleteFriend(int fromUserID, int toUserID);
		void ApproveFriend(int fromUserID, int toUserID);
		bool IsFriend(int fromUserID, int toUserID);
	}
}