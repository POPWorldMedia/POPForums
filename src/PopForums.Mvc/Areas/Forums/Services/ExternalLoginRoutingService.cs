using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Services;

public interface IExternalLoginRoutingService
{
	Dictionary<ProviderType, ExternalLoginTypeMetadata> GetActiveProviderTypeAndNameDictionary();
}

public class ExternalLoginRoutingService : IExternalLoginRoutingService
{
	private readonly ISettingsManager _settingsManager;

	public ExternalLoginRoutingService(ISettingsManager settingsManager)
	{
		_settingsManager = settingsManager;
	}

	public Dictionary<ProviderType, ExternalLoginTypeMetadata> GetActiveProviderTypeAndNameDictionary()
	{
		var dictionary = new Dictionary<ProviderType, ExternalLoginTypeMetadata>();
		if (_settingsManager.Current.UseGoogleLogin)
			dictionary.Add(ProviderType.Google, new ExternalLoginTypeMetadata { Name = "Google", CssClass = "icon-google" });
		if (_settingsManager.Current.UseFacebookLogin)
			dictionary.Add(ProviderType.Facebook, new ExternalLoginTypeMetadata { Name = "Facebook", CssClass = "icon-facebook" });
		if (_settingsManager.Current.UseMicrosoftLogin)
			dictionary.Add(ProviderType.Microsoft, new ExternalLoginTypeMetadata { Name = "Microsoft", CssClass = "icon-microsoft" });
		if (_settingsManager.Current.UseOAuth2Login)
			dictionary.Add(ProviderType.OAuth2, new ExternalLoginTypeMetadata { Name = _settingsManager.Current.OAuth2DisplayName, CssClass = "" });
		return dictionary;
	}
}