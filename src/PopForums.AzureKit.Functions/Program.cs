using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Extensions;
using PopForums.Sql;
using PopForums.Messaging;
using PopForums.Configuration;
using Microsoft.Extensions.Configuration;
using PopForums.ElasticKit;

namespace PopForums.AzureKit.Functions
{
	public class Program
	{
		public static void Main()
		{
			var configuration = new ConfigurationBuilder().Build();
			var config = new Config(configuration);

			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(s =>
				{
					s.AddPopForumsBase();
					s.AddPopForumsSql();
					s.AddPopForumsAzureFunctionsAndQueues();
					s.AddSingleton<IBroker, BrokerSink>();
					
					switch (config.SearchProvider.ToLower())
					{
						case "elasticsearch":
							s.AddPopForumsElasticSearch();
							break;
						case "azuresearch":
							s.AddPopForumsAzureSearch();
							break;
						default:
							break;
					}
				})
				.Build();

			host.Run();
		}
	}
}
