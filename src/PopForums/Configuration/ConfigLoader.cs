namespace PopForums.Configuration;

public class ConfigLoader
{
	public ConfigContainer GetConfig(IConfiguration configuration)
	{
		var container = new ConfigContainer();
		container.DatabaseConnectionString = configuration["PopForums:Database:ConnectionString"];
		var cacheSeconds = configuration["PopForums:Cache:Seconds"];
		container.CacheSeconds = cacheSeconds == null ? 90 : Convert.ToInt32(cacheSeconds);
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
		return container;
	} 
}