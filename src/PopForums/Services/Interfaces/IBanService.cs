namespace PopForums.Services.Interfaces;

public interface IBanService
{
    Task BanIP(string ip);
    Task RemoveIPBan(string ip);
    Task<List<string>> GetIPBans();
    Task BanEmail(string email);
    Task RemoveEmailBan(string email);
    Task<List<string>> GetEmailBans();
}
