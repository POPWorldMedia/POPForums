using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class RoleRepository : IRoleRepository
	{
		public RoleRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public class CacheKeys
		{
			public const string AllRoles = "PopForums.Roles.All";
			public static string UserRole(int userID)
			{
				return "PopForums.Roles.User." + userID;
			}
		}

		public void CreateRole(string role)
		{
			var exists = false;
			_sqlObjectFactory.GetConnection().Using(connection => exists =
				connection.Command(_sqlObjectFactory, "SELECT Role FROM pf_Role WHERE Role LIKE @Role")
					.AddParameter(_sqlObjectFactory, "@Role", role)
					.ExecuteReader().Read());
			if (exists)
				return;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_Role (Role) VALUES (@Role)")
				.AddParameter(_sqlObjectFactory, "@Role", role)
				.ExecuteNonQuery());
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
		}

		public bool DeleteRole(string role)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection => result = 0 != 
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_Role WHERE Role = @Role")
				.AddParameter(_sqlObjectFactory, "@Role", role)
				.ExecuteNonQuery());
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
			return result;
		}

		public List<string> GetAllRoles()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<string>>(CacheKeys.AllRoles);
			if (cacheObject != null)
				return cacheObject;
			var roles = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "SELECT Role FROM pf_Role ORDER BY Role")
					.ExecuteReader()
					.ReadAll(r => roles.Add(r.GetString(0))));
			_cacheHelper.SetCacheObject(CacheKeys.AllRoles, roles);
			return roles;
		}

		public bool IsUserInRole(int userID, string role)
		{
			var roles = GetUserRoles(userID);
			return roles.Contains(role);
		}

		public List<string> GetUserRoles(int userID)
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<string>>(CacheKeys.UserRole(userID));
			if (cacheObject != null)
				return cacheObject;
			var roles = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT Role FROM pf_PopForumsUserRole WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.ExecuteReader()
					.ReadAll(r => roles.Add(r.GetString(0))));
			_cacheHelper.SetCacheObject(CacheKeys.UserRole(userID), roles);
			return roles;
		}

		public List<User> GetUsersInRole(string role)
		{
			List<User> users = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				users = connection.Query<User>("SELECT " + UserRepository.PopForumsUserColumns + " FROM pf_PopForumsUser U JOIN pf_PopForumsUserRole R ON U.UserID = R.UserID WHERE Role = @Role", new { Role = role }).ToList());
			return users;
		}

		public bool AddUserToRole(int userID, string role)
		{
			RemoveUserFromRole(userID, role);
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command(_sqlObjectFactory, "INSERT INTO pf_PopForumsUserRole (UserID, Role) VALUES (@UserID, @Role)")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.AddParameter(_sqlObjectFactory, "@Role", role)
					.ExecuteNonQuery() != 0);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
			return result;
		}

		public bool RemoveUserFromRole(int userID, string role)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command(_sqlObjectFactory, "DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID AND Role = @Role")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.AddParameter(_sqlObjectFactory, "@Role", role)
					.ExecuteNonQuery() != 0);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
			return result;
		}

		public void ReplaceUserRoles(int userID, string[] roles)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.ExecuteNonQuery());
			foreach (var role in roles)
				AddUserToRole(userID, role);
		}
	}
}