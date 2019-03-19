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
			container.QueueConnectionString = config["PopForums:Queue:ConnectionString"];
			return container;
		} 
	}
}