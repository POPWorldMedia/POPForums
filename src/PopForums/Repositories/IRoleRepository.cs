using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Repositories
{
	public interface IRoleRepository
	{
		Task CreateRole(string role);
		Task<bool> DeleteRole(string role);
		Task<List<string>> GetAllRoles();
		Task<List<string>> GetUserRoles(int userID);
		Task ReplaceUserRoles(int userID, string[] roles);
	}
}