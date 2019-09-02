using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Configuration;
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

		public async Task CreateRole(string role)
		{
			Task<IEnumerable<string>> exists = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				exists = connection.QueryAsync<string>("SELECT Role FROM pf_Role WHERE Role LIKE @Role", new { Role = role }));
			if (exists.Result.Any())
				return;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("INSERT INTO pf_Role (Role) VALUES (@Role)", new { Role = role }));
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
		}

		public async Task<bool> DeleteRole(string role)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.ExecuteAsync("DELETE FROM pf_Role WHERE Role = @Role", new { Role = role }));
			_cacheHelper.RemoveCacheObject(CacheKeys.AllRoles);
			return result.Result == 1;
		}

		public async Task<List<string>> GetAllRoles()
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<string>>(CacheKeys.AllRoles);
			if (cacheObject != null)
				return cacheObject;
			Task<IEnumerable<string>> roles = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				roles = connection.QueryAsync<string>("SELECT Role FROM pf_Role ORDER BY Role"));
			_cacheHelper.SetCacheObject(CacheKeys.AllRoles, roles.Result);
			return roles.Result.ToList();
		}

		public async Task<List<string>> GetUserRoles(int userID)
		{
			var cacheObject = _cacheHelper.GetCacheObject<List<string>>(CacheKeys.UserRole(userID));
			if (cacheObject != null)
				return cacheObject;
			Task<IEnumerable<string>> roles = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				roles = connection.QueryAsync<string>("SELECT Role FROM pf_PopForumsUserRole WHERE UserID = @UserID", new { UserID = userID }));
			var list = roles.Result.ToList();
			_cacheHelper.SetCacheObject(CacheKeys.UserRole(userID), list);
			return list;
		}

		private async Task AddUserToRole(int userID, string role)
		{
			await RemoveUserFromRole(userID, role);
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_PopForumsUserRole (UserID, Role) VALUES (@UserID, @Role)", new { UserID = userID, Role = role }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
		}

		private async Task RemoveUserFromRole(int userID, string role)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID AND Role = @Role", new { UserID = userID, Role = role }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserRole(userID));
		}

		public async Task ReplaceUserRoles(int userID, string[] roles)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_PopForumsUserRole WHERE UserID = @UserID", new { UserID = userID }));
			foreach (var role in roles)
				await AddUserToRole(userID, role);
		}
	}
}