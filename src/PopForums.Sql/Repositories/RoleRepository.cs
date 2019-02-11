using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Configuration;
using PopForums.Sql;
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
			_sqlObjectFactory.GetConnection().Using(connection => 
				exists = connection.Query<string>("SELECT Role FROM pf_Role WHERE Role LIKE @Role", new { Role = role }).Any());
			if (exists)
				return;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("INSERT INTO pf_Role (Role) VALUES (@Role)", new { Role = role }));
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
		}

		public bool DeleteRole(string role)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				result = connection.Execute("DELETE FROM pf_Role WHERE Role = @Role", new { Role = role }) == 1);
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
			return result;
		}

		public List<string> GetAllRoles()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<string>>(CacheKeys.AllRoles);
			if (cacheObject != null)
				return cacheObject;
			List<string> roles = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				roles = connection.Query<string>("SELECT Role FROM pf_Role ORDER BY Role").ToList());
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
			List<string> roles = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				roles = connection.Query<string>("SELECT Role FROM pf_PopForumsUserRole WHERE UserID = @UserID", new { UserID = userID }).ToList());
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
				result = connection.Execute("INSERT INTO pf_PopForumsUserRole (UserID, Role) VALUES (@UserID, @Role)", new { UserID = userID, Role = role }) == 1);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
			return result;
		}

		public bool RemoveUserFromRole(int userID, string role)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Execute("DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID AND Role = @Role", new { UserID = userID, Role = role }) == 1);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
			return result;
		}

		public void ReplaceUserRoles(int userID, string[] roles)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID", new { UserID = userID }));
			foreach (var role in roles)
				AddUserToRole(userID, role);
		}
	}
}