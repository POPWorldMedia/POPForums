using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IFriendService
	{
		Task<List<Friend>> GetFriends(User user);
		Task<IEnumerable<Friend>> GetFriendsOf(User user);
		Task<List<User>> GetUnapprovedFriends(User user);
		Task AddUnapprovedFriend(User fromUser, User toUser);
		Task ApproveFriend(User fromUser, User toUser);
		Task DeleteFriend(User fromUser, User toUser);
		Task<bool> IsFriend(User fromUser, User toUser);
	}

	public class FriendService : IFriendService
	{
		private readonly IFriendRepository _friendRepository;

		public FriendService(IFriendRepository friendRepository)
		{
			_friendRepository = friendRepository;
		}

		public async Task<List<Friend>> GetFriends(User user)
		{
			return await _friendRepository.GetFriends(user.UserID);
		}

		public async Task<IEnumerable<Friend>> GetFriendsOf(User user)
		{
			return await _friendRepository.GetFriendsOf(user.UserID);
		}

		public async Task<List<User>> GetUnapprovedFriends(User user)
		{
			return await _friendRepository.GetUnapprovedFriends(user.UserID);
		}

		public async Task AddUnapprovedFriend(User fromUser, User toUser)
		{
			if (fromUser.UserID != toUser.UserID)
				await _friendRepository.AddUnapprovedFriend(fromUser.UserID, toUser.UserID);
		}

		public async Task ApproveFriend(User fromUser, User toUser)
		{
			await _friendRepository.ApproveFriend(fromUser.UserID, toUser.UserID);
		}

		public async Task DeleteFriend(User fromUser, User toUser)
		{
			await _friendRepository.DeleteFriend(fromUser.UserID, toUser.UserID);
		}

		public async Task<bool> IsFriend(User fromUser, User toUser)
		{
			return await _friendRepository.IsFriend(fromUser.UserID, toUser.UserID);
		}
	}
}