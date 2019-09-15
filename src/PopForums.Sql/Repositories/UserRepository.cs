using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class UserRepository : IUserRepository
	{
		public UserRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public class CacheKeys
		{
			public const string UsersOnline = "PopForums.Users.Online";
			public const string TotalUsers = "PopForums.Users.Total";
			public const string PointTotals = "PopForums.Users.Points.";
		}

		public const string PopForumsUserColumns = "pf_PopForumsUser.UserID, pf_PopForumsUser.Name, pf_PopForumsUser.Email, pf_PopForumsUser.CreationDate, pf_PopForumsUser.IsApproved, pf_PopForumsUser.AuthorizationKey";

		private void CacheUser(User user)
		{
			// We only cache users by name because it's the only consistent and repetitive get, looked up via the identity principal.
			var key = "PopForums.User." + user.Name;
			_cacheHelper.SetCacheObject(key, user);
		}

		private void RemoveCacheUser(string name)
		{
			var key = "PopForums.User." + name;
			_cacheHelper.RemoveCacheObject(key);
		}

		private User GetCachedUserByName(string name)
		{
			var key = "PopForums.User." + name;
			return _cacheHelper.GetCacheObject<User>(key);
		}

		public async Task SetHashedPassword(User user, string hashedPassword, Guid salt)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_PopForumsUser SET Password = @Password, Salt = @Salt WHERE UserID = @UserID", new { Password = hashedPassword, Salt = salt, user.UserID }));
		}

		public async Task<Tuple<string, Guid?>> GetHashedPasswordByEmail(string email)
		{
			string hashedPassword = null;
			Guid? saltCheck = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(async connection =>
			{
				var reader = await connection.ExecuteReaderAsync("SELECT Password, Salt FROM pf_PopForumsUser WHERE Email = @Email", new {Email = email});
				if (reader.Read())
				{
			         hashedPassword = reader.GetString(0);
					 saltCheck = reader.NullGuidDbHelper(1);
				}
			});
			return Tuple.Create(hashedPassword, saltCheck);
		}

		public async Task<List<User>> GetUsersFromIDs(IList<int> ids)
		{
			Task<IEnumerable<User>> list = null;
			if (!ids.Any())
				return new List<User>();
			var sql = "SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID IN (" + ids[0];
			foreach (var id in ids.Skip(1))
				sql += ", " + id;
			sql += ")";
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<User>(sql));
			return list.Result.ToList();
		}

		public async Task<int> GetTotalUsers()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.TotalUsers);
			if (cacheObject.HasValue)
				return cacheObject.Value;
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(UserID) FROM pf_PopForumsUser"));
			_cacheHelper.SetCacheObject(CacheKeys.TotalUsers, count.Result);
			return await count;
		}

		private async Task<User> GetUser(string sql, object parameters)
		{
			Task<User> user = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				user = connection.QuerySingleOrDefaultAsync<User>(sql, parameters));
			return await user;
		}

		public async Task<User> GetUser(int userID)
		{
			return await GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID = @UserID", new { UserID = userID });
		}

		public async Task<User> GetUserByName(string name)
		{
			var cachedUser = GetCachedUserByName(name);
			if (cachedUser != null)
				return cachedUser;
			var user = await GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name = @Name", new { Name = name });
			if (user == null)
				return null;
			CacheUser(user);
			return user;
		}

		public async Task<User> GetUserByEmail(string email)
		{
			return await GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email = @Email", new { Email = email });
		}

		public async Task<User> GetUserByAuthorizationKey(Guid key)
		{
			return await GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE AuthorizationKey = @AuthorizationKey", new { AuthorizationKey = key });
		}

		public virtual async Task<User> CreateUser(string name, string email, DateTime creationDate, bool isApproved, string hashedPassword, Guid authorizationKey, Guid salt)
		{
			Task<int> userID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				userID = connection.QuerySingleAsync<int>("INSERT INTO pf_PopForumsUser (Name, Email, CreationDate, IsApproved, AuthorizationKey, Password, Salt) VALUES (@Name, @Email, @CreationDate, @IsApproved, @AuthorizationKey, @Password, @Salt);SELECT CAST(SCOPE_IDENTITY() as int)", new { Name = name, Email = email, CreationDate = creationDate, IsApproved = isApproved, AuthorizationKey = authorizationKey, Password = hashedPassword, Salt = salt }));
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_UserActivity (UserID, LastActivityDate, LastLoginDate) VALUES (@UserID, @LastActivityDate, @LastLoginDate)", new {UserID = userID.Result, LastActivityDate = creationDate, LastLoginDate = creationDate}));
			return new User {UserID = await userID, Name = name, Email = email, CreationDate = creationDate, IsApproved = isApproved, AuthorizationKey = authorizationKey};
		}

		public async Task UpdateLastActivityDate(User user, DateTime newDate)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_UserActivity SET LastActivityDate = @LastActivityDate WHERE UserID = @UserID", new { LastActivityDate = newDate, user.UserID }));
		}

		public async Task UpdateLastLoginDate(User user, DateTime newDate)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_UserActivity SET LastLoginDate = @LastLoginDate WHERE UserID = @UserID", new { LastLoginDate = newDate, user.UserID }));
		}

		public async Task ChangeName(User user, string newName)
		{
			var oldName = user.Name;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_PopForumsUser SET Name = @Name WHERE UserID = @UserID", new { Name = newName, user.UserID }));
			RemoveCacheUser(oldName);
		}

		public async Task ChangeEmail(User user, string newEmail)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_PopForumsUser SET Email = @Email WHERE UserID = @UserID", new { Email = newEmail, user.UserID }));
			RemoveCacheUser(user.Name);
		}

		public async Task UpdateIsApproved(User user, bool isApproved)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_PopForumsUser SET IsApproved = @IsApproved WHERE UserID = @UserID", new { IsApproved = isApproved, user.UserID }));
			RemoveCacheUser(user.Name);
		}

		public async Task UpdateAuthorizationKey(User user, Guid key)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_PopForumsUser SET AuthorizationKey = @AuthorizationKey WHERE UserID = @UserID", new { AuthorizationKey = key, user.UserID }));
			RemoveCacheUser(user.Name);
		}

		public async Task<List<User>> SearchByEmail(string email)
		{
			var list = await GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email LIKE '%' + @Email + '%'", new { Email = email });
			return list;
		}

		public async Task<List<User>> SearchByName(string name)
		{
			var list = await GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name LIKE '%' + @Name + '%'", new { Name = name });
			return list;
		}

		public async Task<List<User>> SearchByRole(string role)
		{
			var list = await GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_PopForumsUserRole R ON pf_PopForumsUser.UserID = R.UserID WHERE Role = @Role", new { Role = role });
			return list;
		}

		public async Task<List<User>> GetUsersOnline()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<User>>(CacheKeys.UsersOnline);
			if (cacheObject != null)
				return cacheObject;
			Task<IEnumerable<User>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<User>("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_UserSession ON pf_PopForumsUser.UserID = pf_UserSession.UserID ORDER BY Name"));
			var userList = list.Result.ToList();
			_cacheHelper.SetCacheObject(CacheKeys.UsersOnline, userList, 60);
			return userList;
		}

		public async Task<List<User>> GetSubscribedUsers()
		{
			Task<IEnumerable<User>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<User>("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_Profile ON pf_PopForumsUser.UserID = pf_Profile.UserID WHERE pf_Profile.IsSubscribed = 1"));
			return list.Result.ToList();
		}

		public Dictionary<User, int> GetUsersByPointTotals(int top)
		{
			var key = CacheKeys.PointTotals + top;
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<User, int>>(key);
			if (cacheObject != null)
				return cacheObject;
			var list = new Dictionary<User, int>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Query<User, int, User>(
					$"SELECT TOP {top} {PopForumsUserColumns}, pf_Profile.Points FROM pf_PopForumsUser JOIN pf_Profile ON pf_PopForumsUser.UserID = pf_Profile.UserID ORDER BY pf_Profile.Points DESC",
					(user, points) =>
					{
						list.Add(user, points);
						return user;
					}, splitOn: "Points"));
			_cacheHelper.SetCacheObject(key, list, 60);
			return list;
		}

		public async Task DeleteUser(User user)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_PopForumsUser WHERE UserID = @UserID", new { user.UserID }));
			RemoveCacheUser(user.Name);
		}

		private async Task<List<User>> GetList(string sql, object parameters)
		{
			if (parameters == null)
				return new List<User>();
			Task<IEnumerable<User>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<User>(sql, parameters));
			return list.Result.ToList();
		}
	}
}
