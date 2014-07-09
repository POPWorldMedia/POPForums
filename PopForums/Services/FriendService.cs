using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class FriendService : IFriendService
	{
		private readonly IFriendRepository _friendRepository;

		public FriendService(IFriendRepository friendRepository)
		{
			_friendRepository = friendRepository;
		}

		public List<Friend> GetFriends(User user)
		{
			return _friendRepository.GetFriends(user.UserID);
		}

		public List<Friend> GetFriendsOf(User user)
		{
			return _friendRepository.GetFriendsOf(user.UserID);
		}

		public List<User> GetUnapprovedFriends(User user)
		{
			return _friendRepository.GetUnapprovedFriends(user.UserID);
		}

		public void AddUnapprovedFriend(User fromUser, User toUser)
		{
			if (fromUser.UserID != toUser.UserID)
				_friendRepository.AddUnapprovedFriend(fromUser.UserID, toUser.UserID);
		}

		public void ApproveFriend(User fromUser, User toUser)
		{
			_friendRepository.ApproveFriend(fromUser.UserID, toUser.UserID);
		}

		public void DeleteFriend(User fromUser, User toUser)
		{
			_friendRepository.DeleteFriend(fromUser.UserID, toUser.UserID);
		}

		public bool IsFriend(User fromUser, User toUser)
		{
			return _friendRepository.IsFriend(fromUser.UserID, toUser.UserID);
		}
	}
}