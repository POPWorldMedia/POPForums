using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class BanService : IBanService
{
	private readonly IBanRepository _banRepository;

	public BanService(IBanRepository banRepsoitory)
	{
		_banRepository = banRepsoitory;
	}

	public async Task BanIP(string ip)
	{
		await _banRepository.BanIP(ip.Trim());
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
		await _banRepository.BanEmail(email.Trim());
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