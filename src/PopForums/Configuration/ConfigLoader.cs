using System;
using Microsoft.Extensions.Configuration;

namespace PopForums.Configuration
{
	public class ConfigLoader
	{
		public ConfigContainer GetConfig(string basePath, string configFileName)
		{
			var builder = new ConfigurationBuilder();
			builder.SetBasePath(basePath);
			builder.AddJsonFile(configFileName, optional: true);
			builder.AddEnvironmentVariables("APPSETTING_");
			var config = builder.Build();
			var container = new ConfigContainer();
			container.DatabaseConnectionString = config["PopForums:Database:ConnectionString"];
			var cacheSeconds = config["PopForums:Cache:Seconds"];
			container.CacheSeconds = cacheSeconds == null ? 90 : Convert.ToInt32(cacheSeconds);
			container.CacheConnectionString = config["PopForums:Cache:ConnectionString"];
			container.CacheForceLocalOnly = Convert.ToBoolean(config["PopForums:Cache:ForceLocalOnly"]);
			container.SearchUrl = config["PopForums:Search:Url"];
			container.SearchKey = config["PopForums:Search:Key"];
			var searchProvider = config["PopForums:Search:Provider"];
			container.SearchProvider = searchProvider ?? string.Empty;
			container.QueueConnectionString = config["PopForums:Queue:ConnectionString"];
			var logTopicViews = config["PopForums:LogTopicViews"];
			container.LogTopicViews = logTopicViews != null && bool.Parse(logTopicViews);
			var useReCaptcha = config["PopForums:ReCaptcha:UseReCaptcha"];
			container.UseReCaptcha = useReCaptcha != null && bool.Parse(useReCaptcha);
			container.ReCaptchaSiteKey = config["PopForums:ReCaptcha:SiteKey"];
			container.ReCaptchaSecretKey = config["PopForums:ReCaptcha:SecretKey"];
			var externalLoginOnly = config["PopForums:ExternalLoginOnly"];
			container.ExternalLoginOnly = externalLoginOnly != null && bool.Parse(externalLoginOnly);
			return container;
		}
	}
}