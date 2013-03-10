using PopForums.Models;

namespace PopForums.Services
{
	public class ClientSettingsMapper : IClientSettingsMapper
	{
		public ClientSettings GetClientSettings(Profile profile)
		{
			var settings = new ClientSettings
			{
			    HideVanity = profile.HideVanity,
			    UsePlainText = profile.IsPlainText
			};
			return settings;
		}

		public ClientSettings GetDefault()
		{
			var settings = new ClientSettings
			{
				HideVanity = false,
				UsePlainText = false
			};
			return settings;
		}
	}
}