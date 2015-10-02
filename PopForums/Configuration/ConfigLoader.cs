using System;
using Microsoft.Framework.Configuration;

namespace PopForums.Configuration
{
	public class ConfigLoader
	{
		public ConfigContainer GetConfig(string basePath)
		{
			if (String.IsNullOrWhiteSpace(basePath))
				throw new ArgumentException("Can't setup PopForums configuration without specifying the base path of the app, where PopForums.json should be found.", nameof(basePath));
			var builder = new ConfigurationBuilder(basePath);
			builder.AddJsonFile("PopForums.json");
			var config = builder.Build();
			var container = new ConfigContainer();
			container.DatabaseConnectionString = config["PopForums:Database:ConnectionString"];
			var cacheSeconds = config["PopForums:Cache:Seconds"];
			container.CacheSeconds = cacheSeconds == null ? 90 : Convert.ToInt32(cacheSeconds);
			return container;
		} 
	}
}