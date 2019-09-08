using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Extensions;
using PopForums.Messaging;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Messaging;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;
using PopIdentity.Extensions;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class ServiceCollections
	{
		/// <summary>
		/// Adds web project services to dependency injection container and authentication for POP Forums. This method 
		/// fails if the ISetupService can't connect to the database or the database isn't set up.
		/// </summary>
		/// <param name="services"></param>
		/// <returns>The updated IServiceCollection.</returns>
		public static IServiceCollection AddPopForumsMvc(this IServiceCollection services)
		{
			return services.AddPopForumsMvc(true);
		}

		/// <summary>
		/// Adds web project services to dependency injection container and authentication for POP Forums. This method 
		/// fails if the ISetupService can't connect to the database or the database isn't set up.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="includePopForumsBaseServices">Indicate false if you intend to call
		/// services.AddPopForumsBase() on your own.</param>
		/// <returns>The updated IServiceCollection.</returns>
		public static IServiceCollection AddPopForumsMvc(this IServiceCollection services, bool includePopForumsBaseServices)
		{
			if (includePopForumsBaseServices)
				services.AddPopForumsBase();
			services.AddHttpContextAccessor();
			services.AddPopIdentity();
			services.AddTransient<IUserRetrievalShim, UserRetrievalShim>();
			services.AddTransient<ITopicViewCountService, TopicViewCountService>();
			services.AddTransient<IExternalLoginRoutingService, ExternalLoginRoutingService>();
			services.AddTransient<IExternalLoginTempService, ExternalLoginTempService>();
			services.AddTransient<IBroker, Broker>();
			// this is required for error logging:
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			
			var serviceProvider = services.BuildServiceProvider();
			var setupService = serviceProvider.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return services;

			services.AddAuthentication()
				.AddCookie(PopForumsAuthorizationDefaults.AuthenticationScheme, option => option.ExpireTimeSpan = new TimeSpan(365, 0, 0, 0));

			return services;
		}
	}
}