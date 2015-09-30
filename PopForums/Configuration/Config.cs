
namespace PopForums.Configuration
{
	public interface IConfig
	{
		string ConnectionStringName { get; }
	}

	public class Config : IConfig
	{
		public Config()
		{
			// TODO: new up config
			//var section = (PopForumsConfigSection)ConfigurationManager.GetSection("popForums");
			//ConnectionStringName = section.ConnectionStringName;
			//CacheSeconds = section.CacheSeconds;
			//CacheConnectionStringName = section.CacheConnectionStringName;
		}

		public string ConnectionStringName { get; private set; }
		public int CacheSeconds { get; private set; }
		public string CacheConnectionStringName { get; private set; }
	}
}
