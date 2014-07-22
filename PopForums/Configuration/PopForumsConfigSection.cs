using System.Configuration;

namespace PopForums.Configuration
{
	public class PopForumsConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("connectionStringName", IsRequired=true)]
		public string ConnectionStringName
		{
			get { return (string) this["connectionStringName"]; }
		}
	
		[ConfigurationProperty("cacheSeconds", IsRequired=false, DefaultValue=90)]
		public int CacheSeconds
		{
			get { return (int) this["cacheSeconds"]; }
		}

		[ConfigurationProperty("cacheConnectionStringName", IsRequired = false)]
		public string CacheConnectionStringName
		{
			get { return (string)this["cacheConnectionStringName"]; }
		}
	}
}
