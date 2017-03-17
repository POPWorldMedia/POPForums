using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Messaging;
using PopForums.Mvc.Areas.Forums.Messaging;
using PopForums.Mvc.Areas.Forums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class ServiceCollections
	{
		/// <summary>
		/// Adds web project services to dependency injection container for POP Forums.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddPopForumsWeb(this IServiceCollection services)
		{
			services.AddTransient<IUserRetrievalShim, UserRetrievalShim>();
			services.AddTransient<ITopicViewCountService, TopicViewCountService>();
			services.AddTransient<IMobileDetectionWrapper, MobileDetectionWrapper>();
			services.AddTransient<IBroker, Broker>();
			// this is required for error logging:
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			return services;
		}
	}
}