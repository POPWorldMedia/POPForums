using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Extensions;
using PopForums.Sql;
using PopForums.Messaging;
using PopForums.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.AzureKit;
using PopForums.AzureKit.Functions;
using PopForums.ElasticKit;
using PopForums.Repositories;
using NotificationTunnel = PopForums.AzureKit.Functions.NotificationTunnel;

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
		s.AddSingleton<ICacheHelper, PopForums.AzureKit.Functions.CacheHelper>();
		s.RemoveAll<INotificationTunnel>();
		s.AddTransient<INotificationTunnel, NotificationTunnel>();
		s.RemoveAll<IPostImageRepository>();
		s.AddTransient<IPostImageRepository, PopForums.AzureKit.PostImage.PostImageRepository>();

		// use Azure table storage for logging instead of database
		//s.AddPopForumsTableStorageLogging();

		switch (config.SearchProvider.ToLower())
		{
			case "elasticsearch":
				s.AddPopForumsElasticSearch();
				break;
			case "azuresearch":
				s.AddPopForumsAzureSearch();
				break;
		}
	})
	.Build();

await host.RunAsync();
