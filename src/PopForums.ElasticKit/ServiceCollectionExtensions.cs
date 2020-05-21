using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PopForums.ElasticKit.Search;
using PopForums.Repositories;
using PopForums.Services;
using SearchIndexSubsystem = PopForums.ElasticKit.Search.SearchIndexSubsystem;

namespace PopForums.ElasticKit
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddPopForumsElasticSearch(this IServiceCollection services)
		{
			services.Replace(ServiceDescriptor.Transient<ISearchRepository, SearchRepository>());
			services.Replace(ServiceDescriptor.Transient<ISearchIndexSubsystem, SearchIndexSubsystem>());
			services.AddTransient<IElasticSearchClientWrapper, ElasticSearchClientWrapper>();
			return services;
		}
	}
}