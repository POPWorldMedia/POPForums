using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Repositories
{
	public interface IBanRepository
	{
		Task BanIP(string ip);
		Task RemoveIPBan(string ip);
		Task<List<string>> GetIPBans();
		Task<bool> IPIsBanned(string ip);
		Task BanEmail(string email);
		Task RemoveEmailBan(string email);
		Task<List<string>> GetEmailBans();
		Task<bool> EmailIsBanned(string email);
	}
}