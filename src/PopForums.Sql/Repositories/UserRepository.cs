using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET Password = @Password, Salt = @Salt WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@Password", hashedPassword)
					.AddParameter(_sqlObjectFactory, "@Salt", salt)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		public string GetHashedPasswordByEmail(string email, out Guid? salt)
		{
			string hashedPassword = null;
			Guid? saltCheck = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "SELECT Password, Salt FROM pf_PopForumsUser WHERE Email = @Email")
					.AddParameter(_sqlObjectFactory, "@Email", email)
					.ExecuteReader()
					.ReadOne(r =>
					         {
						         hashedPassword = r.GetString(0);
								 saltCheck = r.NullGuidDbHelper(1);
					         }));
			salt = saltCheck;
			return hashedPassword;
		}

		public List<User> GetUsersFromIDs(IList<int> ids)
		{
			var list = new List<User>();
			if (ids.Count() == 0)
				return list;
			var sql = "SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID IN (" + ids[0];
			foreach (var id in ids.Skip(1))
				sql += ", " + id;
			sql += ")";
			_sqlObjectFactory.GetConnection().Using(connection =>
			                                 connection.Command(_sqlObjectFactory, sql)
			                                 	.ExecuteReader()
			                                 	.ReadAll(r => list.Add(PopulateUser(r))));
			return list;
		}

		public int GetTotalUsers()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.TotalUsers);
			if (cacheObject.HasValue)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => count = Convert.ToInt32(connection.Command(_sqlObjectFactory, "SELECT COUNT(UserID) FROM pf_PopForumsUser")
						.ExecuteScalar()));
			_cacheHelper.SetCacheObject(CacheKeys.TotalUsers, count);
			return count;
		}

		internal static User PopulateUser(DbDataReader reader)
		{
			return new User(
				reader.GetInt32(0),
				reader.GetDateTime(3)) {
				Name = reader.GetString(1),
				Email = reader.GetString(2),
				IsApproved = reader.GetBoolean(4),
				LastActivityDate = reader.GetDateTime(5),
				LastLoginDate = reader.GetDateTime(6),
				AuthorizationKey = reader.GetGuid(7)};
		}

		private User GetUser(string sql, string parameter, object value)
		{
			User user = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, sql)
					.AddParameter(_sqlObjectFactory, parameter, value)
					.ExecuteReader()
					.ReadOne(r => { user = PopulateUser(r); }));
			return user;
		}

		public User GetUser(int userID)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE UserID = @UserID", "@UserID", userID);
		}

		public User GetUserByName(string name)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name = @Name", "@Name", name);
		}

		public User GetUserByEmail(string email)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email = @Email", "@Email", email);
		}

		public User GetUserByAuthorizationKey(Guid key)
		{
			return GetUser("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE AuthorizationKey = @AuthorizationKey", "@AuthorizationKey", key);
		}

		public virtual User CreateUser(string name, string email, DateTime creationDate, bool isApproved, string hashedPassword, Guid authorizationKey, Guid salt)
		{
			var userID = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				userID = Convert.ToInt32(connection.Command(_sqlObjectFactory, "INSERT INTO pf_PopForumsUser (Name, Email, CreationDate, IsApproved, LastActivityDate, LastLoginDate, AuthorizationKey, Password, Salt) VALUES (@Name, @Email, @CreationDate, @IsApproved, @LastActivityDate, @LastLoginDate, @AuthorizationKey, @Password, @Salt)")
					.AddParameter(_sqlObjectFactory, "@Name", name)
					.AddParameter(_sqlObjectFactory, "@Email", email)
					.AddParameter(_sqlObjectFactory, "@CreationDate", creationDate)
					.AddParameter(_sqlObjectFactory, "@IsApproved", isApproved)
					.AddParameter(_sqlObjectFactory, "@LastActivityDate", creationDate)
					.AddParameter(_sqlObjectFactory, "@LastLoginDate", creationDate)
					.AddParameter(_sqlObjectFactory, "@AuthorizationKey", authorizationKey)
					.AddParameter(_sqlObjectFactory, "@Password", hashedPassword)
					.AddParameter(_sqlObjectFactory, "@Salt", salt)
					.ExecuteAndReturnIdentity()));
			return new User(Convert.ToInt32(userID), creationDate) {Name = name, Email = email, IsApproved = isApproved, LastActivityDate = creationDate, LastLoginDate = creationDate, AuthorizationKey = authorizationKey};
		}

		public void UpdateLastActivityDate(User user, DateTime newDate)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET LastActivityDate = @LastActivityDate WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@LastActivityDate", newDate)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID).ExecuteNonQuery());
		}

		public void UpdateLastLoginDate(User user, DateTime newDate)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
			    connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET LastLoginDate = @LastLoginDate WHERE UserID = @UserID")
			    .AddParameter(_sqlObjectFactory, "@LastLoginDate", newDate)
			    .AddParameter(_sqlObjectFactory, "@UserID", user.UserID).ExecuteNonQuery());
		}

		public void ChangeName(User user, string newName)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET Name = @Name WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@Name", newName)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		public void ChangeEmail(User user, string newEmail)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET Email = @Email WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@Email", newEmail)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		public void UpdateIsApproved(User user, bool isApproved)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET IsApproved = @IsApproved WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@IsApproved", isApproved)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		public void UpdateAuthorizationKey(User user, Guid key)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "UPDATE pf_PopForumsUser SET AuthorizationKey = @AuthorizationKey WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@AuthorizationKey", key)
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		public List<User> SearchByEmail(string email)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Email LIKE '%' + @Email + '%'", "@Email", email);
			return list;
		}

		public List<User> SearchByName(string name)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser WHERE Name LIKE '%' + @Name + '%'", "@Name", name);
			return list;
		}

		public List<User> SearchByRole(string role)
		{
			var list = GetList("SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_PopForumsUserRole R ON pf_PopForumsUser.UserID = R.UserID WHERE Role = @Role", "@Role", role);
			return list;
		}

		public List<User> GetUsersOnline()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<User>>(CacheKeys.UsersOnline);
			if (cacheObject != null)
				return cacheObject;
			var list = new List<User>();
			_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command(_sqlObjectFactory, "SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_UserSession ON pf_PopForumsUser.UserID = pf_UserSession.UserID ORDER BY Name")
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateUser(r))));
			_cacheHelper.SetCacheObject(CacheKeys.UsersOnline, list, 60);
			return list;
		}

		public List<User> GetSubscribedUsers()
		{
			var list = new List<User>();
			_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command(_sqlObjectFactory, "SELECT " + PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_Profile ON pf_PopForumsUser.UserID = pf_Profile.UserID WHERE pf_Profile.IsSubscribed = 1")
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateUser(r))));
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
					connection.Command(_sqlObjectFactory, String.Format("SELECT TOP {0} {1}, pf_Profile.Points FROM pf_PopForumsUser JOIN pf_Profile ON pf_PopForumsUser.UserID = pf_Profile.UserID ORDER BY pf_Profile.Points DESC", top, PopForumsUserColumns))
					.ExecuteReader()
					.ReadAll(r => list.Add(PopulateUser(r), Convert.ToInt32(r["Points"]))));
			_cacheHelper.SetCacheObject(key, list, 60);
			return list;
		}

		public void DeleteUser(User user)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_PopForumsUser WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", user.UserID)
					.ExecuteNonQuery());
		}

		private List<User> GetList(string sql, string parameter, string searchTerm)
		{
			if (searchTerm == null)
				return new List<User>();
			var list = new List<User>();
			_sqlObjectFactory.GetConnection().Using(connection =>
			        connection.Command(_sqlObjectFactory, sql)
			        .AddParameter(_sqlObjectFactory, parameter, searchTerm)
			        .ExecuteReader()
			        .ReadAll(r => list.Add(PopulateUser(r))));
			return list;
		}
	}
}
