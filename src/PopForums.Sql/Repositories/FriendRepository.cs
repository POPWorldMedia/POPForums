using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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

		// TODO: None of this Dapper magic has been tested because friends aren't really used in the app
		// yet. In fact, it probably doesn't even make sense in practice to have a friend object, since you
		// will probably just query for users, approved or not.

		public List<Friend> GetFriends(int userID)
		{
			List<Friend> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<Friend, User, Friend>(
					"SELECT " + UserRepository.PopForumsUserColumns +
					", pf_Friend.IsApproved FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID ORDER BY pf_PopForumsUser.Name",
					(friend, user) =>
					{
						friend.User = user;
						return friend;
					},
					new { FromUserID = userID },
					splitOn: "IsApproved").ToList());
			return list;
		}

		public List<Friend> GetFriendsOf(int userID)
		{
			List<Friend> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<Friend, User, Friend>(
					"SELECT " + UserRepository.PopForumsUserColumns +
					", pf_Friend.IsApproved FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.FromUserID = pf_PopForumsUser.UserID WHERE ToUserID = @ToUserID ORDER BY pf_PopForumsUser.Name",
					(friend, user) =>
					{
						friend.User = user;
						return friend;
					},
					new { ToUserID = userID },
					splitOn: "IsApproved").ToList());
			return list;
		}

		public List<User> GetUnapprovedFriends(int userID)
		{
			List<User> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>("SELECT " + UserRepository.PopForumsUserColumns + " FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID AND NOT pf_Friend.IsApproved = 1 ORDER BY pf_PopForumsUser.Name", new { FromUserID = userID }).ToList());
			return list;
		}

		public void AddUnapprovedFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_Friend (FromUserID, ToUserID, IsApproved) VALUES (@FromUserID, @ToUserID, 0)", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public void DeleteFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public void ApproveFriend(int fromUserID, int toUserID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_Friend SET IsApproved = 1 WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public bool IsFriend(int fromUserID, int toUserID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				result = connection.Query("SELECT * FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID AND IsApproved = 1", new { FromUserID = fromUserID, ToUserID = toUserID }).Any());
			return result;
		}
	}
}