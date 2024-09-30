using PopForums.Mvc.Areas.Forums.BackgroundJobs;
using PopIdentity.Extensions;

namespace PopForums.Mvc.Areas.Forums.Extensions;

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
		services.AddTransient<IUserStateComposer, UserStateComposer>();
		services.AddTransient<IBroker, Broker>();
		services.AddTransient<IUserIdProvider, PopForumsUserIdProvider>();
		services.AddTransient<IOAuthOnlyService, OAuthOnlyService>();
			
		var serviceProvider = services.BuildServiceProvider();
		var setupService = serviceProvider.GetService<ISetupService>();
		if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
			return services;

		services.AddAuthentication()
			.AddCookie(PopForumsAuthorizationDefaults.AuthenticationScheme, option =>
			{
				option.ExpireTimeSpan = new TimeSpan(365, 0, 0, 0);
				option.LoginPath = "/Forums/Account/Login";
				// TODO: This is lame because of fx, see: https://github.com/dotnet/aspnetcore/issues/9039
				option.Events.OnRedirectToAccessDenied = context =>
				{
					context.Response.StatusCode = 403;
					return Task.CompletedTask;
				};
			});

		return services;
	}

	public static IServiceCollection AddPopForumsBackgroundJobs(this IServiceCollection services)
	{
		services.AddHostedService<EmailJob>();
		services.AddHostedService<UserSessionJob>();
		services.AddHostedService<SearchIndexJob>();
		services.AddHostedService<AwardCalculatorJob>();
		services.AddHostedService<CloseAgedTopicsJob>();
		services.AddHostedService<PostImageCleanupJob>();
		services.AddHostedService<SubscribeNotificationJob>();
		return services;
	}
}