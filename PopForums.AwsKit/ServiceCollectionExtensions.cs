using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.AwsKit.Search;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.AwsKit
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddPopForumsElasticSearch(this IServiceCollection services)
		{
			services.Replace(ServiceDescriptor.Transient<ISearchRepository, PopForums.AwsKit.Search.SearchRepository>());
			services.Replace(ServiceDescriptor.Transient<ISearchIndexSubsystem, PopForums.AwsKit.Search.SearchIndexSubsystem>());
			services.AddTransient<IElasticSearchClientWrapper, ElasticSearchClientWrapper>();
			return services;
		}
	}
}