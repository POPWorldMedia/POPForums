using PopForums.Configuration;
using PopForums.Data.SqlSingleWebServer;

namespace PopForums.Data.Azure
{
	public class AzureInjectionRegistry : SqlSingleInjectionRegistry
	{
		public AzureInjectionRegistry()
		{
			For<ICacheHelper>().Use<PopForums.Data.Azure.CacheHelper>();
		}
	}
}