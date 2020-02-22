using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.AzureKit.Queue;
using PopForums.AzureKit.Redis;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.AzureKit
{
    public static class ServiceCollectionExtensions
    {
	    public static IServiceCollection AddPopForumsRedisCache(this IServiceCollection services)
	    {
		    services.AddTransient<ICacheTelemetry, CacheTelemetrySink>();
			var serviceProvider = services.BuildServiceProvider();
			var config = serviceProvider.GetService<IConfig>();
			if (config.ForceLocalOnly)
				return services;
			services.Replace(ServiceDescriptor.Transient<ICacheHelper, PopForums.AzureKit.Redis.CacheHelper>());
		    return services;
	    }

	    public static ISignalRServerBuilder AddRedisBackplaneForPopForums(this ISignalRServerBuilder signalRServerBuilder)
		{
			var serviceProvider = signalRServerBuilder.Services.BuildServiceProvider();
			var config = serviceProvider.GetService<IConfig>();
			signalRServerBuilder.AddStackExchangeRedis(config.CacheConnectionString);
			return signalRServerBuilder;
		}

		public static IServiceCollection AddPopForumsAzureSearch(this IServiceCollection services)
		{
			services.Replace(ServiceDescriptor.Transient<ISearchRepository, PopForums.AzureKit.Search.SearchRepository>());
			services.Replace(ServiceDescriptor.Transient<ISearchIndexSubsystem, PopForums.AzureKit.Search.SearchIndexSubsystem>());
			return services;
		}

		public static IServiceCollection AddPopForumsAzureFunctionsAndQueues(this IServiceCollection services)
		{
			services.Replace(ServiceDescriptor.Transient<IEmailQueueRepository, PopForums.AzureKit.Queue.EmailQueueRepository>());
			services.Replace(ServiceDescriptor.Transient<IAwardCalculationQueueRepository, PopForums.AzureKit.Queue.AwardCalculationQueueRepository>());
			services.Replace(ServiceDescriptor.Transient<ISearchIndexQueueRepository, PopForums.AzureKit.Queue.SearchIndexQueueRepository>());
			return services;
		}
	}
}
