using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFriendRepository
	{
		SortedList<User, bool> GetFriends(int userID);
		SortedList<User, bool> GetFriendsOf(int userID);
		List<User> GetUnapprovedFriends(int userID);
		void AddUnapprovedFriend(int fromUserID, int toUserID);
		void DeleteFriend(int fromUserID, int toUserID);
		void ApproveFriend(int fromUserID, int toUserID);
		bool IsFriend(int fromUserID, int toUserID);
	}
}