using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class FriendRepository : IFriendRepository
	{
		public FriendRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		// TODO: None of this Dapper magic has been tested because friends aren't really used in the app
		// yet. In fact, it probably doesn't even make sense in practice to have a friend object, since you
		// will probably just query for users, approved or not.

		public async Task<List<Friend>> GetFriends(int userID)
		{
			Task<IEnumerable<Friend>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<Friend, User, Friend>(
					"SELECT " + UserRepository.PopForumsUserColumns +
					", pf_Friend.IsApproved FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID ORDER BY pf_PopForumsUser.Name",
					(friend, user) =>
					{
						friend.User = user;
						return friend;
					},
					new { FromUserID = userID },
					splitOn: "IsApproved"));
			return list.Result.ToList();
		}

		public async Task<IEnumerable<Friend>> GetFriendsOf(int userID)
		{
			Task<IEnumerable<Friend>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<Friend, User, Friend>(
					"SELECT " + UserRepository.PopForumsUserColumns +
					", pf_Friend.IsApproved FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.FromUserID = pf_PopForumsUser.UserID WHERE ToUserID = @ToUserID ORDER BY pf_PopForumsUser.Name",
					(friend, user) =>
					{
						friend.User = user;
						return friend;
					},
					new { ToUserID = userID },
					splitOn: "IsApproved"));
			return list.Result.ToList();
		}

		public async Task<List<User>> GetUnapprovedFriends(int userID)
		{
			Task<IEnumerable<User>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<User>("SELECT " + UserRepository.PopForumsUserColumns + " FROM pf_Friend JOIN pf_PopForumsUser ON pf_Friend.ToUserID = pf_PopForumsUser.UserID WHERE FromUserID = @FromUserID AND NOT pf_Friend.IsApproved = 1 ORDER BY pf_PopForumsUser.Name", new { FromUserID = userID }));
			return list.Result.ToList();
		}

		public async Task AddUnapprovedFriend(int fromUserID, int toUserID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_Friend (FromUserID, ToUserID, IsApproved) VALUES (@FromUserID, @ToUserID, 0)", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public async Task DeleteFriend(int fromUserID, int toUserID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public async Task ApproveFriend(int fromUserID, int toUserID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Friend SET IsApproved = 1 WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID", new { FromUserID = fromUserID, ToUserID = toUserID }));
		}

		public async Task<bool> IsFriend(int fromUserID, int toUserID)
		{
			Task<IEnumerable<dynamic>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.QueryAsync("SELECT * FROM pf_Friend WHERE FromUserID = @FromUserID AND ToUserID = @ToUserID AND IsApproved = 1", new { FromUserID = fromUserID, ToUserID = toUserID }));
			return result.Result.Any();
		}
	}
}