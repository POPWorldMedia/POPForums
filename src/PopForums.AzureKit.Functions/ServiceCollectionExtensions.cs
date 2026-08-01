using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.Configuration;
using PopForums.Messaging;

namespace PopForums.AzureKit.Functions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPopForumsFunctionsHost(this IServiceCollection services)
	{
		services.AddSingleton<IBroker, BrokerSink>();
		services.RemoveAll<ICacheHelper>();
		services.AddSingleton<ICacheHelper, CacheHelper>();
		services.RemoveAll<INotificationTunnel>();
		services.AddTransient<INotificationTunnel, NotificationTunnel>();
		return services;
	}
}
