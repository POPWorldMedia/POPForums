namespace PopForums.Configuration;

public interface IConfig
{
	string DatabaseConnectionString { get; }
	int CacheSeconds { get; }
	string CacheConnectionString { get; }
	bool ForceLocalOnly { get; }
	string SearchUrl { get; }
	string SearchKey { get; }
	string QueueConnectionString { get; }
	string SearchProvider { get; }
	bool LogTopicViews { get; }
	bool UseReCaptcha { get; }
	string ReCaptchaSiteKey { get; }
	string ReCaptchaSecretKey { get; }
	string IpLookupUrlFormat { get; }
	string WebAppUrlAndArea { get; }
	string BaseImageBlobUrl { get; }
	string StorageConnectionString { get; }
	bool RenderBootstrap { get; }
	bool IsOAuthOnly { get; }
	string OAuthClientID { get; }
	string OAuthClientSecret { get; }
	string OAuthLoginBaseUrl { get; }
	string OAuthTokenUrl { get; }
	string OAuthAdminClaimName { get; }
	string OAuthAdminClaimValue { get; }
	string OAuthModeratorClaimName { get; }
	string OAuthModeratorClaimValue { get; }
	String OAuthScopes { get; }
}

public class Config : IConfig
{
	public Config(IConfiguration configuration)
	{
		if (_configContainer == null)
		{
			var loader = new ConfigLoader();
			_configContainer = loader.GetConfig(configuration);
		}
	}

	private static ConfigContainer _configContainer;

	public string DatabaseConnectionString => _configContainer.DatabaseConnectionString;
	public int CacheSeconds => _configContainer.CacheSeconds;
	public string CacheConnectionString => _configContainer.CacheConnectionString;
	public bool ForceLocalOnly => _configContainer.CacheForceLocalOnly;
	public string SearchUrl => _configContainer.SearchUrl;
	public string SearchKey => _configContainer.SearchKey;
	public string QueueConnectionString => _configContainer.QueueConnectionString;
	public string SearchProvider => _configContainer.SearchProvider;
	public bool LogTopicViews => _configContainer.LogTopicViews;
	public bool UseReCaptcha => _configContainer.UseReCaptcha;
	public string ReCaptchaSiteKey => _configContainer.ReCaptchaSiteKey;
	public string ReCaptchaSecretKey => _configContainer.ReCaptchaSecretKey;
	public string IpLookupUrlFormat => _configContainer.IpLookupUrlFormat;
	public string WebAppUrlAndArea => _configContainer.WebAppUrlAndArea;
	public string BaseImageBlobUrl => _configContainer.BaseImageBlobUrl;
	public string StorageConnectionString => _configContainer.StorageConnectionString;
	public bool RenderBootstrap => _configContainer.RenderBootstrap;
	public bool IsOAuthOnly => _configContainer.IsOAuthOnly;
	public string OAuthClientID => _configContainer.OAuthClientID;
	public string OAuthClientSecret => _configContainer.OAuthClientSecret;
	public string OAuthLoginBaseUrl => _configContainer.OAuthLoginBaseUrl;
	public string OAuthTokenUrl => _configContainer.OAuthTokenUrl;
	public string OAuthAdminClaimName => _configContainer.OAuthAdminClaimName;
	public string OAuthAdminClaimValue => _configContainer.OAuthAdminClaimValue;
	public string OAuthModeratorClaimName => _configContainer.OAuthModeratorClaimName;
	public string OAuthModeratorClaimValue => _configContainer.OAuthModeratorClaimValue;
	public string OAuthScopes => _configContainer.OAuthScopes;
}
