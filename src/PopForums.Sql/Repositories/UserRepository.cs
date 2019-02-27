using System;
using System.Collections.Generic;
using System.Linq;
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

		public const string PopForumsUserColumns = "pf_PopForumsUser.UserID, pf_PopForumsUser.Name, pf_PopForumsUser.Email, pf_PopForumsUser.CreationDate, pf_PopForumsUser.IsApproved, pf_PopForumsUser.LastActivityDate, pf_PopForumsUser.LastLoginDate, pf_PopForumsUser.AuthorizationKey";

		public void SetHashedPassword(User user, string hashedPassword, Guid salt)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_PopForumsUser SET Password = @Password, Salt = @Salt WHERE UserID = @UserID", new { Password = hashedPassword, Salt = salt, user.UserID }));
		}

		public string GetHashedPasswordByEmail(string email, out Guid? salt)
		{
			string hashedPassword = null;
			Guid? saltCheck = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
			{
				var reader = connection.ExecuteReader("SELECT Password, Salt FROM pf_PopForumsUser WHERE Email = @Email", new {Email = email});
				if (reader.Read())
				{
			         hashedPassword = reader.GetString(0);
					 saltCheck = reader.NullGuidDbHelper(1);
				}
			});
			salt = saltCheck;
			return hashedPassword;
		}

		public List<User> GetUsersFromIDs(IList<int> ids)
		{
			List<User> list = null;
			if (!ids.Any())
				return new List<User>();
			var sql = "SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID IN (" + ids[0];
			foreach (var id in ids.Skip(1))
				sql += ", " + id;
			sql += ")";
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>(sql).ToList());
			return list;
		}

		public int GetTotalUsers()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.TotalUsers);
			if (cacheObject.HasValue)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				count = connection.ExecuteScalar<int>("SELECT COUNT(UserID) FROM pf_PopForumsUser"));
			_cacheHelper.SetCacheObject(CacheKeys.TotalUsers, count);
			return count;
		}

		private User GetUser(string sql, object parameters)
		{
			User user = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				user = connection.QuerySingleOrDefault<User>(sql, parameters));
			return user;
		}

		public User GetUser(int userID)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID = @UserID", new { UserID = userID });
		}

		public User GetUserByName(string name)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name = @Name", new { Name = name });
		}

		public User GetUserByEmail(string email)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email = @Email", new { Email = email });
		}

		public User GetUserByAuthorizationKey(Guid key)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE AuthorizationKey = @AuthorizationKey", new { AuthorizationKey = key });
		}

		public virtual User CreateUser(string name, string email, DateTime creationDate, bool isApproved, string hashedPassword, Guid authorizationKey, Guid salt)
		{
			var userID = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				userID = connection.QuerySingle<int>("INSERT INTO pf_PopForumsUser (Name, Email, CreationDate, IsApproved, LastActivityDate, LastLoginDate, AuthorizationKey, Password, Salt) VALUES (@Name, @Email, @CreationDate, @IsApproved, @LastActivityDate, @LastLoginDate, @AuthorizationKey, @Password, @Salt);SELECT CAST(SCOPE_IDENTITY() as int)", new { Name = name, Email = email, CreationDate = creationDate, IsApproved = isApproved, LastActivityDate = creationDate, LastLoginDate = creationDate, AuthorizationKey = authorizationKey, Password = hashedPassword, Salt = salt }));
			return new User {UserID = userID, Name = name, Email = email, CreationDate = creationDate, IsApproved = isApproved, LastActivityDate = creationDate, LastLoginDate = creationDate, AuthorizationKey = authorizationKey};
		}

		public void UpdateLastActivityDate(User user, DateTime newDate)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_PopForumsUser SET LastActivityDate = @LastActivityDate WHERE UserID = @UserID", new { LastActivityDate = newDate, user.UserID }));
		}

		public void UpdateLastLoginDate(User user, DateTime newDate)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_PopForumsUser SET LastLoginDate = @LastLoginDate WHERE UserID = @UserID", new { LastLoginDate = newDate, user.UserID }));
		}

		public void ChangeName(User user, string newName)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_PopForumsUser SET Name = @Name WHERE UserID = @UserID", new { Name = newName, user.UserID }));
		}

		public void ChangeEmail(User user, string newEmail)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_PopForumsUser SET Email = @Email WHERE UserID = @UserID", new { Email = newEmail, user.UserID }));
		}

		public void UpdateIsApproved(User user, bool isApproved)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_PopForumsUser SET IsApproved = @IsApproved WHERE UserID = @UserID", new { IsApproved = isApproved, user.UserID }));
		}

		public void UpdateAuthorizationKey(User user, Guid key)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_PopForumsUser SET AuthorizationKey = @AuthorizationKey WHERE UserID = @UserID", new { AuthorizationKey = key, user.UserID }));
		}

		public List<User> SearchByEmail(string email)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email LIKE '%' + @Email + '%'", new { Email = email });
			return list;
		}

		public List<User> SearchByName(string name)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name LIKE '%' + @Name + '%'", new { Name = name });
			return list;
		}

		public List<User> SearchByRole(string role)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_PopForumsUserRole R ON pf_PopForumsUser.UserID = R.UserID WHERE Role = @Role", new { Role = role });
			return list;
		}

		public List<User> GetUsersOnline()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<User>>(CacheKeys.UsersOnline);
			if (cacheObject != null)
				return cacheObject;
			List<User> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_UserSession ON pf_PopForumsUser.UserID = pf_UserSession.UserID ORDER BY Name").ToList());
			_cacheHelper.SetCacheObject(CacheKeys.UsersOnline, list, 60);
			return list;
		}

		public List<User> GetSubscribedUsers()
		{
			List<User> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_Profile ON pf_PopForumsUser.UserID = pf_Profile.UserID WHERE pf_Profile.IsSubscribed = 1").ToList());
			return list;
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

		public void DeleteUser(User user)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_PopForumsUser WHERE UserID = @UserID", new { user.UserID }));
		}

		private List<User> GetList(string sql, object parameters)
		{
			if (parameters == null)
				return new List<User>();
			List<User> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>(sql, parameters).ToList());
			return list;
		}
	}
}
