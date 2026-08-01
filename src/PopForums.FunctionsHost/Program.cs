using System;
using Microsoft.Extensions.Hosting;
using PopForums.Configuration;
using Microsoft.Extensions.Configuration;
using PopForums.Extensions;
using PopForums.Sql;
using PopForums.AzureKit;
using PopForums.AzureKit.Functions;
using PopForums.ElasticKit;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Environment.CurrentDirectory)
	.AddJsonFile("local.settings.json", true)
	.AddJsonFile("local.settings.dev.json", true)
	.AddEnvironmentVariables()
	.Build();
var config = new Config(configuration);

var host = new HostBuilder()
	.UseDefaultServiceProvider((_, options) =>
	{
		// there are types not used in functions in core library, so don't choke on them
		options.ValidateOnBuild = false;
	})
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureAppConfiguration(c =>
	{
		c.AddConfiguration(configuration);
	})
	.ConfigureServices(s =>
	{
		// set up the dependencies for the SQL library in POP Forums
		s.AddPopForumsBase();
		s.AddPopForumsSql();

		// route background work to Azure queues, and wire up the broker/cache/notification
		// tunnel needed to talk back to the web front end
		s.AddPopForumsAzureFunctionsAndQueues();
		s.AddPopForumsFunctionsHost();

		// persist image uploads to Azure blob storage, see configuration
		s.AddPopForumsAzureBlobStorageForPostImages();

		// use Azure table storage for logging instead of database
		//s.AddPopForumsTableStorageLogging();

		switch (config.SearchProvider.ToLower())
		{
			case "elasticsearch":
			case "elasticcloud":
				s.AddPopForumsElasticSearch();
				Console.WriteLine("ElasticSearch provider configured.");
				break;
			case "azuresearch":
				s.AddPopForumsAzureSearch();
				Console.WriteLine("Azure Search provider configured.");
				break;
			default:
				Console.WriteLine("Default SQL based search provider configured.");
				break;
		}
	})
	.Build();

await host.RunAsync();
