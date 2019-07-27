using System.Collections.Generic;
using PopForums.Configuration;
using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public interface IExternalLoginRoutingService
	{
		Dictionary<ProviderType, string> GetActiveProviderTypeAndNameDictionary();
	}

	public class ExternalLoginRoutingService : IExternalLoginRoutingService
	{
		private readonly ISettingsManager _settingsManager;

		public ExternalLoginRoutingService(ISettingsManager settingsManager)
		{
			_settingsManager = settingsManager;
		}

		public Dictionary<ProviderType, string> GetActiveProviderTypeAndNameDictionary()
		{
			var dictionary = new Dictionary<ProviderType, string>();
			if (_settingsManager.Current.UseGoogleLogin)
				dictionary.Add(ProviderType.Google, "Google");
			if (_settingsManager.Current.UseFacebookLogin)
				dictionary.Add(ProviderType.Facebook, "Facebook");
			if (_settingsManager.Current.UseMicrosoftLogin)
				dictionary.Add(ProviderType.Microsoft, "Microsoft");
			if (_settingsManager.Current.UseOAuth2Login)
				dictionary.Add(ProviderType.OAuth2, _settingsManager.Current.OAuth2DisplayName);
			return dictionary;
		}
	}
}