using System;
using Microsoft.Extensions.Configuration;

namespace PopForums.Configuration
{
	public class ConfigLoader
	{
		public ConfigContainer GetConfig(string basePath)
		{
			if (String.IsNullOrWhiteSpace(basePath))
				throw new ArgumentException("Can't setup PopForums configuration without specifying the base path of the app, where PopForums.json should be found.", nameof(basePath));
			var builder = new ConfigurationBuilder();
			builder.SetBasePath(basePath);
			builder.AddJsonFile("PopForums.json");
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
			container.QueueConnectionString = config["PopForums:Queue:ConnectionString"];
			return container;
		} 
	}
}