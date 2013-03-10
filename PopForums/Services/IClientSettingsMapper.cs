using PopForums.Models;

namespace PopForums.Services
{
	public interface IClientSettingsMapper
	{
		ClientSettings GetClientSettings(Profile profile);
		ClientSettings GetDefault();
	}
}