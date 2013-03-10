using System.Configuration;

namespace PopForums.Configuration
{
	public class Config : IConfig
	{
		public Config()
		{
			var section = (PopForumsConfigSection)ConfigurationManager.GetSection("popForums");
			ConnectionStringName = section.ConnectionStringName;
			CacheSeconds = section.CacheSeconds;
		}

		public string ConnectionStringName { get; private set; }
		public int CacheSeconds { get; private set; }
	}
}
