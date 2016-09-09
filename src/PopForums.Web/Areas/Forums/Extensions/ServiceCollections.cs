using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Web.Areas.Forums.Authorization;
using PopForums.Web.Areas.Forums.Messaging;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public static class ServiceCollections
	{
		/// <summary>
		/// Configures the POP Forums role policies for Admin and Moderators.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection ConfigurePopForumsAuthorizationPolicies(this IServiceCollection services)
		{
			services.Configure<AuthorizationOptions>(options =>
			{
				options.AddPolicy(PermanentRoles.Admin, policy => policy.RequireClaim(PopForumsAuthorizationDefaults.ForumsClaimType, PermanentRoles.Admin));
				options.AddPolicy(PermanentRoles.Moderator, policy => policy.RequireClaim(PopForumsAuthorizationDefaults.ForumsClaimType, PermanentRoles.Moderator));
			});
			return services;
		}

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
			return services;
		}
	}
}