using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IRoleRepository
	{
		void CreateRole(string role);
		bool DeleteRole(string role);
		List<string> GetAllRoles();
		bool IsUserInRole(int userID, string role);
		List<string> GetUserRoles(int userID);
		List<User> GetUsersInRole(string role);
		bool AddUserToRole(int userID, string role);
		bool RemoveUserFromRole(int userID, string role);
		void ReplaceUserRoles(int userID, string[] roles);
	}
}