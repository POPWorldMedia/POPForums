using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Extensions;
using PopForums.Sql;
using PopForums.Messaging;
using PopForums.Configuration;
using System;

namespace PopForums.AzureKit.Functions
{
	public class Program
	{
		public static void Main()
		{
			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(s =>
				{
					s.AddPopForumsBase();
					s.AddPopForumsSql();
					s.AddPopForumsAzureFunctionsAndQueues();
					s.AddSingleton<IBroker, BrokerSink>();

					//string searchType;
					//var config = s.GetService<IConfig>();
					//switch (config.SearchProvider.ToLower())
					//{
					//	case "elasticsearch":
					//		searchType = "ElasticSearch (PopForums.ElasticKit)";
					//		services.AddPopForumsElasticSearch();
					//		break;
					//	case "azuresearch":
					//		searchType = "Azure Search (PopForums.AzureKit)";
					//		services.AddPopForumsAzureSearch();
					//		break;
					//	default:
					//		searchType = "Default (PopForums.Sql)";
					//		break;
					//}
				})
				.Build();

			host.Run();
		}
	}
}
