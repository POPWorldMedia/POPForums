using PopForums.Configuration;
using PopForums.Data.SqlSingleWebServer;

namespace PopForums.Data.Azure
{
	public class AzureInjectionModule : SqlSingleInjectionModule
	{
		public override void Load()
		{
			base.Load();
			Unbind<ICacheHelper>();
			Bind<ICacheHelper>().To<PopForums.Data.Azure.CacheHelper>();
		}
	}
}