using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.Messaging;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Messaging;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class ServiceCollections
	{
		/// <summary>
		/// Adds web project services to dependency injection container and authentication for POP Forums. This method 
		/// fails if the ISetupService can't connect to the database or the database isn't set up.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddPopForumsMvc(this IServiceCollection services)
		{
			services.AddTransient<IUserRetrievalShim, UserRetrievalShim>();
			services.AddTransient<ITopicViewCountService, TopicViewCountService>();
			services.AddTransient<IMobileDetectionWrapper, MobileDetectionWrapper>();
			services.AddTransient<IBroker, Broker>();
			// this is required for error logging:
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			
			var serviceProvider = services.BuildServiceProvider();
			var setupService = serviceProvider.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return services;
			var settingsManager = serviceProvider.GetService<ISettingsManager>();
			var settings = settingsManager.Current;

			var authenticationBuilder = services.AddAuthentication()
				.AddCookie(PopForumsAuthorizationDefaults.AuthenticationScheme, option => option.ExpireTimeSpan = new TimeSpan(365, 0, 0, 0));

			if (settings.UseTwitterLogin)
				authenticationBuilder.AddTwitter(x =>
				{
					x.ConsumerKey = settings.TwitterConsumerKey;
					x.ConsumerSecret = settings.TwitterConsumerSecret;
				});

			if (settings.UseFacebookLogin)
				authenticationBuilder.AddFacebook(x =>
				{
					x.AppId = settings.FacebookAppID;
					x.AppSecret = settings.FacebookAppSecret;
				});

			if (settings.UseGoogleLogin)
				authenticationBuilder.AddGoogle(x =>
				{
					x.ClientId = settings.GoogleClientId;
					x.ClientSecret = settings.GoogleClientSecret;
				});

			if (settings.UseMicrosoftLogin)
				authenticationBuilder.AddMicrosoftAccount(x =>
				{
					x.ClientId = settings.MicrosoftClientID;
					x.ClientSecret = settings.MicrosoftClientSecret;
				});

			return services;
		}
	}
}