using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IBanService
	{
		Task BanIP(string ip);
		Task RemoveIPBan(string ip);
		Task<List<string>> GetIPBans();
		Task BanEmail(string email);
		Task RemoveEmailBan(string email);
		Task<List<string>> GetEmailBans();
	}

	public class BanService : IBanService
	{
		public BanService(IBanRepository banRepsoitory)
		{
			_banRepository = banRepsoitory;
		}

		private readonly IBanRepository _banRepository;

		public async Task BanIP(string ip)
		{
			await _banRepository.BanIP(ip);
		}

		public async Task RemoveIPBan(string ip)
		{
			await _banRepository.RemoveIPBan(ip);
		}

		public async Task<List<string>> GetIPBans()
		{
			return await _banRepository.GetIPBans();
		}

		public async Task BanEmail(string email)
		{
			await _banRepository.BanEmail(email);
		}

		public async Task RemoveEmailBan(string email)
		{
			await _banRepository.RemoveEmailBan(email);
		}

		public async Task<List<string>> GetEmailBans()
		{
			return await _banRepository.GetEmailBans();
		}
	}
}