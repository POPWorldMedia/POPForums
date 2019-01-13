using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class FriendServiceTests
	{
		private FriendService GetService()
		{
			_friendRepo = new Mock<IFriendRepository>();
			return new FriendService(_friendRepo.Object);
		}

		private Mock<IFriendRepository> _friendRepo;

		[Fact]
		public void GetFriendsCallsRepoByUserIDAndReturnsList()
		{
			var service = GetService();
			var list = new List<Friend>();
			var user = new User { UserID = 123 };
			_friendRepo.Setup(x => x.GetFriends(user.UserID)).Returns(list);
			
			var result = service.GetFriends(user);

			Assert.Same(result, list);
			_friendRepo.Verify(x => x.GetFriends(user.UserID), Times.Once());
		}

		[Fact]
		public void GetUnapprovedFriendsCallsRepoByUserIDAndReturnsList()
		{
			var service = GetService();
			var list = new List<User>();
			var user = new User { UserID = 123 };
			_friendRepo.Setup(x => x.GetUnapprovedFriends(user.UserID)).Returns(list);

			var result = service.GetUnapprovedFriends(user);

			Assert.Same(result, list);
			_friendRepo.Verify(x => x.GetUnapprovedFriends(user.UserID), Times.Once());
		}

		[Fact]
		public void AddUnapprovedFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User { UserID = 123 };
			var toUser = new User { UserID = 456 };

			service.AddUnapprovedFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.AddUnapprovedFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Fact]
		public void AddUnapprovedFriendWontCallRepoIfSelfAddingFriend()
		{
			var service = GetService();
			var fromUser = new User { UserID = 123 };

			service.AddUnapprovedFriend(fromUser, fromUser);

			_friendRepo.Verify(x => x.AddUnapprovedFriend(fromUser.UserID, fromUser.UserID), Times.Never());
		}

		[Fact]
		public void ApproveFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User { UserID = 123 };
			var toUser = new User { UserID = 456 };

			service.ApproveFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.ApproveFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Fact]
		public void DeleteFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User { UserID = 123 };
			var toUser = new User { UserID = 456 };

			service.DeleteFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.DeleteFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Fact]
		public void IsFriendCallsRepoByIDsAndReturnsResult()
		{
			var service = GetService();
			var fromUser = new User { UserID = 123 };
			var toUser = new User { UserID = 456 };
			_friendRepo.Setup(x => x.IsFriend(fromUser.UserID, toUser.UserID)).Returns(true);

			var result = service.IsFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.IsFriend(fromUser.UserID, toUser.UserID), Times.Once());
			Assert.True(result);
		}
	}
}