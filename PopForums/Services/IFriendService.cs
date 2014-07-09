using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IFriendService
	{
		List<Friend> GetFriends(User user);
		List<Friend> GetFriendsOf(User user);
		List<User> GetUnapprovedFriends(User user);
		void AddUnapprovedFriend(User fromUser, User toUser);
		void ApproveFriend(User fromUser, User toUser);
		void DeleteFriend(User fromUser, User toUser);
		bool IsFriend(User fromUser, User toUser);
	}
}