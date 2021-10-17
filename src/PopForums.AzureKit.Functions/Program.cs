using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Extensions;
using PopForums.Sql;
using PopForums.Messaging;
using PopForums.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.ElasticKit;

namespace PopForums.AzureKit.Functions
{
	public class Program
	{
		public static async Task Main()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Environment.CurrentDirectory)
				.AddJsonFile("local.settings.json", true)
				.AddEnvironmentVariables()
				.Build();
			var config = new Config(configuration);

			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureAppConfiguration(c =>
				{
					c.AddConfiguration(configuration);
				})
				.ConfigureServices(s =>
				{
					s.AddPopForumsBase();
					s.AddPopForumsSql();
					s.AddPopForumsAzureFunctionsAndQueues();
					s.AddSingleton<IBroker, BrokerSink>();
					s.RemoveAll<ICacheHelper>();
					s.AddSingleton<ICacheHelper, CacheHelper>();
					
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

			await host.RunAsync();
		}
	}
}
