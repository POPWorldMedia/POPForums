namespace PopForums.Configuration;

public class ConfigLoader
{
	public ConfigContainer GetConfig(IConfiguration configuration)
	{
		var container = new ConfigContainer();
		container.DatabaseConnectionString = configuration["PopForums:Database:ConnectionString"];
		var cacheSeconds = configuration["PopForums:Cache:Seconds"];
		container.CacheSeconds = string.IsNullOrEmpty(cacheSeconds) ? 90 : Convert.ToInt32(cacheSeconds);
		container.CacheConnectionString = configuration["PopForums:Cache:ConnectionString"];
		container.CacheForceLocalOnly = Convert.ToBoolean(configuration["PopForums:Cache:ForceLocalOnly"]);
		container.SearchUrl = configuration["PopForums:Search:Url"];
		container.SearchKey = configuration["PopForums:Search:Key"];
		var searchProvider = configuration["PopForums:Search:Provider"];
		container.SearchProvider = searchProvider ?? string.Empty;
		container.QueueConnectionString = configuration["PopForums:Queue:ConnectionString"];
		var logTopicViews = configuration["PopForums:LogTopicViews"];
		container.LogTopicViews = logTopicViews != null && bool.Parse(logTopicViews);
		var useReCaptcha = configuration["PopForums:ReCaptcha:UseReCaptcha"];
		container.UseReCaptcha = useReCaptcha != null && bool.Parse(useReCaptcha);
		container.ReCaptchaSiteKey = configuration["PopForums:ReCaptcha:SiteKey"];
		container.ReCaptchaSecretKey = configuration["PopForums:ReCaptcha:SecretKey"];
		container.IpLookupUrlFormat = configuration["PopForums:IpLookupUrlFormat"];
		container.WebAppUrlAndArea = configuration["PopForums:WebAppUrlAndArea"];
		container.BaseImageBlobUrl = configuration["PopForums:BaseImageBlobUrl"];
		container.StorageConnectionString = configuration["PopForums:Storage:ConnectionString"];
		var renderBootstrap = configuration["PopForums:RenderBootstrap"];
		container.RenderBootstrap = renderBootstrap != null ? bool.Parse(renderBootstrap) : true;
		var isOAuthOnly = configuration["PopForums:OAuthOnly:IsOAuthOnly"];
		container.IsOAuthOnly = isOAuthOnly != null ? bool.Parse(isOAuthOnly) : false;
		container.OAuthClientID = configuration["PopForums:OAuthOnly:OAuthClientID"];
		container.OAuthClientSecret = configuration["PopForums:OAuthOnly:OAuthClientSecret"];
		container.OAuthLoginBaseUrl = configuration["PopForums:OAuthOnly:OAuthLoginBaseUrl"];
		container.OAuthTokenUrl = configuration["PopForums:OAuthOnly:OAuthTokenUrl"];
		container.OAuthAdminClaimType = configuration["PopForums:OAuthOnly:OAuthAdminClaimType"];
		container.OAuthAdminClaimValue = configuration["PopForums:OAuthOnly:OAuthAdminClaimValue"];
		container.OAuthModeratorClaimType = configuration["PopForums:OAuthOnly:OAuthModeratorClaimType"];
		container.OAuthModeratorClaimValue = configuration["PopForums:OAuthOnly:OAuthModeratorClaimValue"];
		container.OAuthScopes = configuration["PopForums:OAuthOnly:OAuthScopes"];
		var refreshMinutes = configuration["PopForums:OAuthOnly:OAuthRefreshExpirationMinutes"];
		container.OAuthRefreshExpirationMinutes = string.IsNullOrEmpty(refreshMinutes) ? 60 : Convert.ToInt32(refreshMinutes);
		
		return container;
	} 
}