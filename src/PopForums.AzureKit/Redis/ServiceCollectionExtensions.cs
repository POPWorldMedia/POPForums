using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.Configuration;

namespace PopForums.AzureKit.Redis
{
    public static class ServiceCollectionExtensions
    {
	    public static IServiceCollection AddPopForumsRedisCache(this IServiceCollection services)
	    {
		    services.Replace(ServiceDescriptor.Transient<ICacheHelper, CacheHelper>());
		    return services;
	    }
    }
}
