using System;
using Microsoft.Framework.Configuration;

namespace PopForums.Configuration
{
	public class ConfigLoader
	{
		public ConfigContainer GetConfig()
		{
			var builder = new ConfigurationBuilder();
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