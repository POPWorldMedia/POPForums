using System.Collections.Generic;

namespace PopForums.Services
{
	public interface IBanService
	{
		void BanIP(string ip);
		void RemoveIPBan(string ip);
		List<string> GetIPBans();
		bool IPIsBanned(string ip);
		void BanEmail(string email);
		void RemoveEmailBan(string email);
		List<string> GetEmailBans();
		bool EmailIsBanned(string email);
	}
}