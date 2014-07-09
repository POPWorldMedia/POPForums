using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class FriendServiceTests
	{
		private FriendService GetService()
		{
			_friendRepo = new Mock<IFriendRepository>();
			return new FriendService(_friendRepo.Object);
		}

		private Mock<IFriendRepository> _friendRepo;

		[Test]
		public void GetFriendsCallsRepoByUserIDAndReturnsList()
		{
			var service = GetService();
			var list = new List<Friend>();
			var user = new User(123, DateTime.MinValue);
			_friendRepo.Setup(x => x.GetFriends(user.UserID)).Returns(list);
			
			var result = service.GetFriends(user);

			Assert.AreSame(result, list);
			_friendRepo.Verify(x => x.GetFriends(user.UserID), Times.Once());
		}

		[Test]
		public void GetUnapprovedFriendsCallsRepoByUserIDAndReturnsList()
		{
			var service = GetService();
			var list = new List<User>();
			var user = new User(123, DateTime.MinValue);
			_friendRepo.Setup(x => x.GetUnapprovedFriends(user.UserID)).Returns(list);

			var result = service.GetUnapprovedFriends(user);

			Assert.AreSame(result, list);
			_friendRepo.Verify(x => x.GetUnapprovedFriends(user.UserID), Times.Once());
		}

		[Test]
		public void AddUnapprovedFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User(123, DateTime.MinValue);
			var toUser = new User(456, DateTime.MinValue);

			service.AddUnapprovedFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.AddUnapprovedFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Test]
		public void AddUnapprovedFriendWontCallRepoIfSelfAddingFriend()
		{
			var service = GetService();
			var fromUser = new User(123, DateTime.MinValue);

			service.AddUnapprovedFriend(fromUser, fromUser);

			_friendRepo.Verify(x => x.AddUnapprovedFriend(fromUser.UserID, fromUser.UserID), Times.Never());
		}

		[Test]
		public void ApproveFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User(123, DateTime.MinValue);
			var toUser = new User(456, DateTime.MinValue);

			service.ApproveFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.ApproveFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Test]
		public void DeleteFriendCallsRepoByIDs()
		{
			var service = GetService();
			var fromUser = new User(123, DateTime.MinValue);
			var toUser = new User(456, DateTime.MinValue);

			service.DeleteFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.DeleteFriend(fromUser.UserID, toUser.UserID), Times.Once());
		}

		[Test]
		public void IsFriendCallsRepoByIDsAndReturnsResult()
		{
			var service = GetService();
			var fromUser = new User(123, DateTime.MinValue);
			var toUser = new User(456, DateTime.MinValue);
			_friendRepo.Setup(x => x.IsFriend(fromUser.UserID, toUser.UserID)).Returns(true);

			var result = service.IsFriend(fromUser, toUser);

			_friendRepo.Verify(x => x.IsFriend(fromUser.UserID, toUser.UserID), Times.Once());
			Assert.IsTrue(result);
		}
	}
}