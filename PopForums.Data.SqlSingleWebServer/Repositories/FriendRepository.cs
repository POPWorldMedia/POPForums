using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class FriendRepository : IFriendRepository
	{
		public FriendRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public List<Friend> GetFriends(int userID)
		{
			var list = new List<Friend>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT " + UserRepository.PopForumsUserColumns + ", pf_Friend.IsApproved AS App FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID ORDER BY pf_PopForumsUser.Name")
				.AddParameter("@FromUserID", userID)
				.ExecuteReader()
				.ReadAll(r => list.Add(
					new Friend {User = UserRepository.PopulateUser(r), IsApproved = (bool)r["App"]})));
			return list;
		}

		public List<Friend> GetFriendsOf(int userID)
		{
			var list = new List<Friend>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT " + UserRepository.PopForumsUserColumns + ", pf_Friend.IsApproved AS App FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.FromUserID = pf_PopForumsUser.UserID WHERE ToUserID = @ToUserID ORDER BY pf_PopForumsUser.Name")
				.AddParameter("@ToUserID", userID)
				.ExecuteReader()
				.ReadAll(r => list.Add(
					new Friend { User = UserRepository.PopulateUser(r), IsApproved = (bool)r["App"] })));
			return list;
		}

		public List<User> GetUnapprovedFriends(int userID)
		{
			var list = new List<User>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT " + UserRepository.PopForumsUserColumns + " FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID AND NOT pf_Friend.IsApproved = 1 ORDER BY pf_PopForumsUser.Name")
				.AddParameter("@FromUserID", userID)
				.ExecuteReader()
				.ReadAll(r => list.Add(
					UserRepository.PopulateUser(r))));
			return list;
		}

		public void AddUnapprovedFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("INSERT INTO pf_Friend (FromUserID, ToUserID, IsApproved) VALUES (@FromUserID, @ToUserID, 0)")
				.AddParameter("@FromUserID", fromUserID)
				.AddParameter("@ToUserID", toUserID)
				.ExecuteNonQuery());
		}

		public void DeleteFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("DELETE FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID")
				.AddParameter("@FromUserID", fromUserID)
				.AddParameter("@ToUserID", toUserID)
				.ExecuteNonQuery());
		}

		public void ApproveFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("UPDATE pf_Friend SET IsApproved = 1 WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID")
				.AddParameter("@FromUserID", fromUserID)
				.AddParameter("@ToUserID", toUserID)
				.ExecuteNonQuery());
		}

		public bool IsFriend(int fromUserID, int toUserID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(c => result = Convert.ToInt32(
				c.Command("SELECT COUNT(*) FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID AND IsApproved = 1")
				.AddParameter("@FromUserID", fromUserID)
				.AddParameter("@ToUserID", toUserID)
				.ExecuteScalar()) > 0);
			return result;
		}
	}
}